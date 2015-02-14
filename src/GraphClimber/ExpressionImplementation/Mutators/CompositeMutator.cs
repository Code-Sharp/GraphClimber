using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace GraphClimber
{
    internal class CompositeMutator : IMethodMutator
    {
        private readonly IEnumerable<IMethodMutator> _mutators;

        public CompositeMutator(IEnumerable<IMethodMutator> mutators)
        {
            _mutators = mutators;
        }

        public Expression Mutate(Expression oldValue, Expression processor, Expression owner, IStateMember member,
            Expression descriptor)
        {
            Expression result = oldValue;
            
            foreach (var mutator in _mutators)
            {
                result = mutator.Mutate(result, processor, owner, member, descriptor);
            }

            return result;
        }
    }
}