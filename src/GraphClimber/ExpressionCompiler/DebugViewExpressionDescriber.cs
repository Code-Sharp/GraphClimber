using System.Linq.Expressions;
using System.Reflection;

namespace GraphClimber.ExpressionCompiler
{
    public class DebugViewExpressionDescriber : IExpressionDescriber
    {
        public static readonly IExpressionDescriber Empty = new DebugViewExpressionDescriber();

        private static readonly PropertyInfo _debugView = typeof(Expression).GetProperty("DebugView", BindingFlags.NonPublic | BindingFlags.Instance);

        private DebugViewExpressionDescriber()
        {
            
        }

        private static string GetDebugView(Expression expression)
        {
            return (string) _debugView.GetValue(expression);
        }

        public string Describe(Expression expression)
        {
            return GetDebugView(expression);
        }
    }
}