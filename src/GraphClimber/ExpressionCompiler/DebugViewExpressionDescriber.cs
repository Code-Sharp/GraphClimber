using System.Linq.Expressions;
using System.Reflection;

namespace GraphClimber.ExpressionCompiler
{
    /// <summary>
    /// Implementation of <see cref="IExpressionDescriber"/>
    /// using the property "DebugView" of the expressions.
    /// 
    /// This may not work if microsoft/mono changes the implementation
    /// of "DebugView".
    /// </summary>
    public class DebugViewExpressionDescriber : IExpressionDescriber
    {
        public static readonly IExpressionDescriber Empty = new DebugViewExpressionDescriber();

        private static readonly PropertyInfo _debugView = typeof(Expression).GetProperty("DebugView", BindingFlags.NonPublic | BindingFlags.Instance);

        /// <summary>
        /// Private C'tor, Please use <see cref="Empty"/> property to get the instance of <see cref="DebugViewExpressionDescriber"/>.
        /// </summary>
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