using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

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
                Expression callProcessor = CallProcessor(castedProcessor, owner, member, descriptorVariables, expressions);
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
         IStateMember member,
         List<ParameterExpression> methodVariables,
         List<Expression> methodAssignments)
        {
            DescriptorWriter writer = new DescriptorWriter(_climbStore);

            DescriptorVariable descriptor =
                writer.GetDescriptor(castedProcessor, owner, member, member.MemberType,
                                     _stateMemberProvider);

            Expression callProcessor =
                _mutator.GetExpression(castedProcessor, owner, member, descriptor.Reference);

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

            int[] indices = new int[ranks];
            var member = _stateMemberProvider.ProvideArrayMember(runtimeType, indices);

            List<ParameterExpression> variables = new List<ParameterExpression>();
            List<Expression> assignments = new List<Expression>();

            Expression callExpression =
                CallProcessor(castedProcessor, owner, member, variables, assignments);

            var loop = LoopArrayBounds(ranks, owner, Expression.Constant(indices), callExpression, variables, assignments);

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
         Expression owner,
         Expression stateMemberArray,
         Expression callExpression,
         List<ParameterExpression> parameterExpressions,
         List<Expression> assignments)
        {
            var rankParameters =
                Enumerable.Range(0, ranks).Select(r => Expression.Variable(typeof(int), "rank_" + r)).ToList();

            var upperBoundParameters =
                Enumerable.Range(0, ranks).Select(r => Expression.Variable(typeof(int), "upper_" + r)).ToList();


            for (int rank = ranks - 1; rank >= 0; rank--)
            {
                // Create a for loop from lowerBound to upperBound?
                var breakTarget = Expression.Label("break" + rank);
                var continueTarget = Expression.Label("continue" + rank);

                // i_k = array.GetLowerBound(k)
                var currentLoopVariableAssignment =
                    Expression.Assign(rankParameters[rank],
                                      Expression.Call(owner, "GetLowerBound", null,
                                                      Expression.Constant(rank)));

                // length_k = array.GetUpperBound(k)
                var currentUpperBoundAssignment = Expression.Assign(upperBoundParameters[rank],
                                                                   Expression.Call(owner, "GetUpperBound", null,
                                                                                   Expression.Constant(rank)));

                assignments.Add(currentUpperBoundAssignment);

                Expression binaryExpression = Expression.ArrayAccess(stateMemberArray,
                                                             Expression.Constant(rank));
                Expression setIndexer =
                    Expression.Assign(binaryExpression,
                                      rankParameters[rank]);

                Expression setIndexerAndCallMethod = 
                    Expression.Block(setIndexer, callExpression);

                var loopBody =
                    Expression.Block(
                                     Expression.IfThen(
                                                       Expression.GreaterThan(rankParameters[rank], upperBoundParameters[rank]),
                                                       Expression.Goto(breakTarget)),
                                     setIndexerAndCallMethod,
                                     Expression.PostIncrementAssign(rankParameters[rank]),
                                     Expression.Goto(continueTarget));

                callExpression = 
                    Expression.Block(currentLoopVariableAssignment,
                    Expression.Loop(loopBody, breakTarget, continueTarget));
            }

            parameterExpressions.AddRange(rankParameters);
            parameterExpressions.AddRange(upperBoundParameters);

            return callExpression;
        }
    }
}