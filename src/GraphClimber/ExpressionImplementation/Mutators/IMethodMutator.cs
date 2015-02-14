using System.Linq.Expressions;

namespace GraphClimber
{
    internal interface IMethodMutator
    {
        Expression Mutate(Expression oldValue,
            Expression processor,
            Expression owner,
            IStateMember member,
            Expression descriptor);
    }
}