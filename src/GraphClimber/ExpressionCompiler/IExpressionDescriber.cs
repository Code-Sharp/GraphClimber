using System.Linq.Expressions;

namespace GraphClimber.ExpressionCompiler
{
    public interface IExpressionDescriber
    {

        string Describe(Expression expression);

    }
}