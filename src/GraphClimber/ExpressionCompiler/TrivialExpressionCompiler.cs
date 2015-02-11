using System.Linq.Expressions;

namespace GraphClimber.ExpressionCompiler
{
    public class TrivialExpressionCompiler : IExpressionCompiler
    {
        public TDelegate Compile<TDelegate>(Expression<TDelegate> expression)
        {
            return expression.Compile();
        }
    }
}