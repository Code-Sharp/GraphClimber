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

        public Expression Mutate(Expression oldExpression, Expression processor, Expression value, Expression owner, IStateMember member, Expression descriptor)
        {
            Expression result = oldExpression;
            
            foreach (var mutator in _mutators)
            {
                result = mutator.Mutate(result, processor, value, owner, member, descriptor);
            }

            return result;
        }
    }
}