using System.Linq.Expressions;

namespace GraphClimber
{
    internal interface IMethodMutator
    {
        Expression Mutate(Expression oldExpression,
            Expression processor,
            Expression value,
            Expression owner,
            IStateMember member,
            Expression descriptor);
    }
}