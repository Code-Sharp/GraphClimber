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
        private CallProcessMutator _mutator;

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
                return CreateObjectDelegate<T>(runtimeType);
            }
        }

        private ClimbDelegate<T> CreateObjectDelegate<T>(Type runtimeType)
        {
            var processor = Expression.Parameter(typeof(object), "processor");
            var value = Expression.Parameter(typeof (T), "value");

            Expression castedProcessor = processor.Convert(_processorType);
            Expression owner = value.Convert(runtimeType);

            IEnumerable<IStateMember> members =
                _stateMemberProvider.Provide(runtimeType);

            List<Expression> expressions = new List<Expression>();
            List<ParameterExpression> descriptorVariables = new List<ParameterExpression>();

            foreach (IStateMember member in members)
            {
                DescriptorWriter writer = new DescriptorWriter(_climbStore);

                DescriptorVariable descriptor =
                    writer.GetDescriptor(castedProcessor, owner, member, member.MemberType);

                Expression callProcessor =
                    _mutator.GetExpression(castedProcessor, owner, member, descriptor.Reference);

                descriptorVariables.Add(descriptor.Reference);
                expressions.Add(descriptor.Declaration);
                expressions.Add(callProcessor);
            }

            BlockExpression climbBody = 
                Expression.Block(descriptorVariables, expressions);

            Expression<ClimbDelegate<T>> lambda =
                Expression.Lambda<ClimbDelegate<T>>(climbBody,
                    "Climb_" + runtimeType.Name,
                    new[] {processor, value});

            ClimbDelegate<T> result = _compiler.Compile(lambda);

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