using System;
using System.Linq.Expressions;

namespace GraphClimber
{
    internal class CallProcessMutator
    {
        private readonly CompositeMutator _mutator;

        public CallProcessMutator(Type processorType, IMethodMapper mapper)
        {
            _mutator = new CompositeMutator(new IMethodMutator[]
            {
                new MethodCallMutator(processorType, mapper, false),
                new PolymorphismMutator(),
                new RevisitedMutator(processorType),
                new NullProcessorMutator(processorType)
            });
        }

        public Expression GetExpression
            (Expression processor,
                Expression value,
                Expression owner,
                IStateMember member,
                Expression descriptor)
        {
            return _mutator.Mutate(Expression.Empty(), processor, value, owner, member, descriptor);
        }
    }
}