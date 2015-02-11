using System.Linq.Expressions;

namespace GraphClimber.ExpressionCompiler
{
    public interface IExpressionCompiler
    {

        TDelegate Compile<TDelegate>(Expression<TDelegate> expression);

    }
}
