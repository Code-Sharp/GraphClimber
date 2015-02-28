using System.Linq.Expressions;

namespace GraphClimber
{
    internal interface IMethodMutator
    {
        Expression Mutate(Expression oldExpression,
            Expression processor,
            Expression owner,
            IStateMember member,
            Expression descriptor);
    }
}