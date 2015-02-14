using System;
using System.Linq.Expressions;

namespace GraphClimber
{
    internal class SpecialMethodMutator : IMethodMutator
    {
        private readonly CompositeMutator _mutator;

        public SpecialMethodMutator(Type processorType)
        {
            _mutator =
                new CompositeMutator(new IMethodMutator[]
                {
                    new RevisitedMutator(processorType),
                    new NullProcessorMutator(processorType)
                });
        }

        public Expression Mutate(Expression oldValue, Expression processor, Expression owner, IStateMember member,
            Expression descriptor)
        {
            return _mutator.Mutate(oldValue, processor, owner, member, descriptor);
        }
    }
}