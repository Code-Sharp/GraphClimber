using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using GraphClimber.ExpressionCompiler;
using GraphClimber.ExpressionCompiler.Extensions;

namespace GraphClimber
{
    internal class ClimbDelegateFactory
    {
        private readonly Type _processorType;
        private readonly IStateMemberProvider _stateMemberProvider;
        private readonly IMethodMapper _methodMapper;
        private readonly ClimbStore _climbStore;
        private readonly IExpressionCompiler _compiler;
        private readonly CallProcessMutator _mutator;

        public ClimbDelegateFactory(Type processorType, IStateMemberProvider stateMemberProvider, IMethodMapper methodMapper, ClimbStore climbStore, IExpressionCompiler compiler)
        {
            _processorType = processorType;
            _stateMemberProvider = stateMemberProvider;
            _methodMapper = methodMapper;
            _climbStore = climbStore;
            _compiler = compiler;
            _mutator = new CallProcessMutator(_processorType, _methodMapper);
        }

        public ClimbDelegate<T> CreateDelegate<T>(Type runtimeType)
        {
            if (runtimeType.IsArray)
            {
                return CreateArrayDelegate<T>(runtimeType);
            }
            else
            {
                return CreateReferenceDelegate<T>(runtimeType);
            }
        }


        private ClimbDelegate<T> CreateReferenceDelegate<T>(Type runtimeType)
        {
            var processor = Expression.Parameter(typeof(object), "processor");
            var value = Expression.Parameter(typeof (T), "value");

            Expression owner = value.Convert(runtimeType);
            BlockExpression climbBody = 
                GetClimbBody(runtimeType, processor, owner, owner);

            Expression<ClimbDelegate<T>> lambda =
                Expression.Lambda<ClimbDelegate<T>>(climbBody,
                    "Climb_" + runtimeType.Name,
                    new[] {processor, value});

            ClimbDelegate<T> result = _compiler.Compile(lambda);

            return result;
        }

        private BlockExpression GetClimbBody(Type runtimeType, Expression processor, Expression ownerForRoute, Expression ownerValue)
        {
            Expression castedProcessor = processor.Convert(_processorType);

            IEnumerable<IStateMember> members =
                _stateMemberProvider.Provide(runtimeType);

            List<Expression> expressions = new List<Expression>();
            List<ParameterExpression> descriptorVariables = new List<ParameterExpression>();

            foreach (IStateMember member in members)
            {
                DescriptorWriter writer = new DescriptorWriter(_climbStore);

                DescriptorVariable descriptor =
                    writer.GetDescriptor(castedProcessor, ownerForRoute, member, member.MemberType);

                Expression value;

                if (member.CanRead)
                {
                    value = member.GetGetExpression(ownerValue);                    
                }
                else
                {
                    value = Expression.Empty();
                }

                Expression callProcessor =
                    _mutator.GetExpression(castedProcessor,
                        value,
                        ownerForRoute,
                        member,
                        descriptor.Reference);

                descriptorVariables.Add(descriptor.Reference);
                expressions.Add(descriptor.Declaration);
                expressions.Add(callProcessor);
            }

            BlockExpression climbBody =
                Expression.Block(descriptorVariables, expressions);
            
            return climbBody;
        }

        public StructClimbDelegate<TField> CreateStructDelegate<TField>(Type structType)
        {
            // Ok! whats going to happen here:
            // 1) Get the value and cast it 
            //      TStruct value = (TStruct)box.Value;
            // 2) Rebox it:
            //      Box<TStruct> reboxed = new Box<TStruct>(value);
            // 3) Do the regular climb thingy. Now ownerForRoute is "reboxed", ownerValue is reboxed.Value (step 2).
            // 4) Assign the reboxed value to the original value:
            //      box.Value = reboxed.Value;
            // 5) Get outta here.
            // (TODO: you can avoid the 2nd and 4th step if structType == typeof(T))
            var processor = Expression.Parameter(typeof(object), "processor");
            var box = Expression.Parameter(typeof(Box<TField>), "box");

            Expression value = Expression.Field(box, "Value");

            Type boxType = typeof(Box<>).MakeGenericType(structType);

            ParameterExpression reboxed = Expression.Variable(boxType, "reboxed");

            ConstructorInfo boxCtor = 
                boxType.GetConstructors().FirstOrDefault();

            Expression assignReboxed =
                Expression.Assign(reboxed,
                    Expression.New(boxCtor, value.Convert(structType)));

            Expression ownerValue = Expression.Field(reboxed, "Value");

            BlockExpression climbBody =
                GetClimbBody(structType, processor, reboxed, ownerValue);

            Expression assignBox =
                Expression.Assign(value, ownerValue.Convert<TField>());

            BlockExpression body =
                Expression.Block(new[] {reboxed},
                    assignReboxed,
                    climbBody,
                    assignBox);

            Expression<StructClimbDelegate<TField>> lambda =
                Expression.Lambda<StructClimbDelegate<TField>>
                    (body,
                        "StructClimb_" + structType.Name,
                        new[] {processor, box});

            StructClimbDelegate<TField> result = 
                _compiler.Compile(lambda);

            return result;
        }

        private ClimbDelegate<T> CreateArrayDelegate<T>(Type runtimeType)
        {
            var processor = Expression.Parameter(typeof(object), "processor");
            var value = Expression.Parameter(typeof (T), "value");

            Expression castedProcessor = processor.Convert(_processorType);
            Expression owner = value.Convert(runtimeType);

            var ranks = runtimeType.GetArrayRank();

            var rankParameters =
                Enumerable.Range(0, ranks).Select(r => Expression.Variable(typeof (int), "rank_" + r)).ToList();

            var upperBoundParameters =
                Enumerable.Range(0, ranks).Select(r => Expression.Variable(typeof (int), "upper_" + r)).ToList();

            var assignRankParameters = new Expression[ranks];
            var assignUpperParameters = new Expression[ranks];
            

            Expression callExpression = Expression.Empty(); // TODO : Complete.

            for (int rank = ranks - 1; rank >= 0; rank--)
            {
                // Create a for loop from lowerBound to upperBound?
                var breakTarget = Expression.Label("break");
                var continueTarget = Expression.Label("continue");

                assignRankParameters[rank] = Expression.Assign(rankParameters[rank],
                    Expression.Call(owner, "GetLowerBound", null, Expression.Constant(rank)));

                assignUpperParameters[rank] = Expression.Assign(upperBoundParameters[rank],
                    Expression.Call(owner, "GetUpperBound", null, Expression.Constant(rank)));

                var loopBody =
                    Expression.Block(
                        Expression.IfThen(
                            Expression.Equal(upperBoundParameters[rank], rankParameters[rank]),
                            Expression.Goto(breakTarget)),
                        callExpression,
                        Expression.PostIncrementAssign(rankParameters[rank]),
                        Expression.Goto(continueTarget));

                callExpression = Expression.Loop(loopBody, breakTarget, continueTarget);
            }

            BlockExpression climbBody =
                Expression.Block(rankParameters.Concat(upperBoundParameters),
                    assignRankParameters.Concat(assignUpperParameters).Concat(new[] {callExpression}));

            Expression<ClimbDelegate<T>> lambda =
                Expression.Lambda<ClimbDelegate<T>>(climbBody,
                    "Climb_" + runtimeType.Name,
                    new[] { processor, value });

            ClimbDelegate<T> result = _compiler.Compile(lambda);

            return result;

            throw new NotImplementedException("CreateArrayDelegate is not implemented");
        }
    }
}