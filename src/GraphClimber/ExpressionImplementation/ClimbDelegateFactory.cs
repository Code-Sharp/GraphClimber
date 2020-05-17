using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace GraphClimber
{
    internal class ClimbDelegateFactory
    {
        private readonly Type _processorType;
        private readonly IStateMemberProvider _stateMemberProvider;
        private readonly IMethodMapper _methodMapper;
        private readonly ClimbStore _climbStore;
        private readonly CallProcessMutator _mutator;

        public ClimbDelegateFactory(Type processorType, IStateMemberProvider stateMemberProvider, IMethodMapper methodMapper, ClimbStore climbStore)
        {
            _processorType = processorType;
            _stateMemberProvider = stateMemberProvider;
            _methodMapper = methodMapper;
            _climbStore = climbStore;
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
            var processor = Expression.Parameter(typeof (object), "processor");
            var value = Expression.Parameter(typeof (T), "value");

            Expression owner = value.Convert(runtimeType);

            Expression castedProcessor = processor.Convert(_processorType);

            IEnumerable<IStateMember> members =
                _stateMemberProvider.Provide(runtimeType);

            List<Expression> expressions = new List<Expression>();
            List<ParameterExpression> descriptorVariables = new List<ParameterExpression>();

            foreach (IStateMember member in members)
            {
                Expression callProcessor = CallProcessor(castedProcessor, owner, EmptyIndex.Constant, member, descriptorVariables, expressions);
                expressions.Add(callProcessor);
            }

            BlockExpression climbBody =
                Expression.Block(descriptorVariables, expressions);

            Expression<ClimbDelegate<T>> lambda =
                Expression.Lambda<ClimbDelegate<T>>(climbBody,
                    "Climb_" + runtimeType.Name,
                    new[] {processor, value});

            ClimbDelegate<T> result = lambda.Compile();

            return result;
        }

        private Expression CallProcessor
        (Expression castedProcessor,
         Expression owner,
         Expression indices,
         IStateMember member,
         List<ParameterExpression> methodVariables,
         List<Expression> methodAssignments)
        {
            DescriptorWriter writer = new DescriptorWriter(_climbStore);

            DescriptorVariable descriptor =
                writer.GetDescriptor(castedProcessor, owner, indices, member, member.MemberType,
                                     _stateMemberProvider);

            Expression callProcessor =
                _mutator.GetExpression(castedProcessor, owner, member, descriptor.Reference, indices);

            methodVariables.Add(descriptor.Reference);
            methodAssignments.Add(descriptor.Declaration);

            return callProcessor;
        }


        private ClimbDelegate<T> CreateArrayDelegate<T>(Type runtimeType)
        {
            var processor = Expression.Parameter(typeof(object), "processor");
            var value = Expression.Parameter(typeof (T), "value");

            Expression castedProcessor = processor.Convert(_processorType);
            Expression owner = value.Convert(runtimeType);

            var ranks = runtimeType.GetArrayRank();

            List<ParameterExpression> variables = new List<ParameterExpression>();
            List<Expression> assignments = new List<Expression>();

            //int[] indices = new int[ranks];
            ParameterExpression indicesArray = Expression.Variable(typeof(int[]), "arrayIndex");
            variables.Add(indicesArray);
            assignments.Add(Expression.Assign(indicesArray, Expression.NewArrayBounds(typeof(int), ranks.Constant())));

            IStateMember member = _stateMemberProvider.ProvideArrayMember(runtimeType, ranks);

            Expression callExpression =
                CallProcessor(castedProcessor, owner, indicesArray, member, variables, assignments);

            
            var loop = LoopArrayBounds(ranks, indicesArray, owner, callExpression, variables, assignments);

            BlockExpression climbBody =
                Expression.Block(variables,
                                 assignments.Concat(new[] {loop}));

            Expression<ClimbDelegate<T>> lambda =
                Expression.Lambda<ClimbDelegate<T>>(climbBody,
                                                    "Climb_" + runtimeType.Name,
                                                    new[] { processor, value });

            ClimbDelegate<T> result = lambda.Compile();

            return result;
        }

        private static Expression LoopArrayBounds
        (int ranks,
         ParameterExpression indicesArray,
         Expression owner,
         Expression callExpression,
         List<ParameterExpression> parameterExpressions,
         List<Expression> assignments)
        {
            var loopVariables =
                Enumerable.Range(0, ranks).Select(r => Expression.ArrayAccess(indicesArray, Expression.Constant(r))).ToList();

            var upperBoundParameters =
                Enumerable.Range(0, ranks).Select(r => Expression.Variable(typeof(int), "upper_" + r)).ToList();


            for (int rank = ranks - 1; rank >= 0; rank--)
            {
                // Create a for loop from lowerBound to upperBound?
                var breakTarget = Expression.Label("break" + rank);
                var continueTarget = Expression.Label("continue" + rank);

                // i_k = array.GetLowerBound(k)
                var currentLoopVariableAssignment =
                    Expression.Assign(loopVariables[rank],
                                      Expression.Call(owner, "GetLowerBound", null,
                                                      Expression.Constant(rank)));

                // length_k = array.GetUpperBound(k)
                var currentUpperBoundAssignment = Expression.Assign(upperBoundParameters[rank],
                                                                   Expression.Call(owner, "GetUpperBound", null,
                                                                                   Expression.Constant(rank)));

                assignments.Add(currentUpperBoundAssignment);

                var loopBody =
                    Expression.Block(
                                     Expression.IfThen(
                                                       Expression.GreaterThan(loopVariables[rank], upperBoundParameters[rank]),
                                                       Expression.Goto(breakTarget)),
                                     callExpression,
                                     Expression.PostIncrementAssign(loopVariables[rank]),
                                     Expression.Goto(continueTarget));

                callExpression = 
                    Expression.Block(currentLoopVariableAssignment,
                    Expression.Loop(loopBody, breakTarget, continueTarget));
            }

            parameterExpressions.AddRange(upperBoundParameters);

            return callExpression;
        }
    }
}