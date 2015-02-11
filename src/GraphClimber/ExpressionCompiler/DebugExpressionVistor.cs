using System;
using System.Linq;
using System.Linq.Expressions;

namespace GraphClimber.ExpressionCompiler
{
    public class DebugExpressionVistor : ExpressionVisitor
    {
        private readonly SymbolDocumentInfo _symbolDocumentInfo;
        private readonly IExpressionDescriber _expressionDescriber;
        private readonly string _initialDebugView;
        private int _currentIndex;

        public DebugExpressionVistor(SymbolDocumentInfo symbolDocumentInfo, Expression initialExpression, IExpressionDescriber expressionDescriber)
        {
            _symbolDocumentInfo = symbolDocumentInfo;
            _expressionDescriber = expressionDescriber;

            _initialDebugView = _expressionDescriber.Describe(initialExpression);
        }


        public override Expression Visit(Expression node)
        {
            if (node == null)
            {
                return null;
            }

            // Don't modify constant expressions (Also ParameterExpressions and more should be added later)
            if (node is ConstantExpression || node is ParameterExpression)
            {
                return node;
            }

            if (node is BlockExpression || node is LambdaExpression || node is LoopExpression)
            {
                return base.Visit(node);
            }


            string debugView = _expressionDescriber.Describe(node);
            var debugViewLines = debugView.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            var firstDebugViewLine = debugViewLines.First();

            _currentIndex = _initialDebugView.IndexOf(firstDebugViewLine, _currentIndex, StringComparison.Ordinal);

            var start = _initialDebugView.GetPosition(_currentIndex);
            var end = _initialDebugView.GetPosition(_currentIndex + debugView.Length + (debugViewLines.Length - 1) * start.Column);


            var innerExpression = base.Visit(node);

            Expression debugInfoExpression = Expression.DebugInfo(_symbolDocumentInfo, start.Line + 1, start.Column + 1, end.Line + 1, end.Column + 1);

            return Expression.Block(debugInfoExpression, innerExpression);

        }
    }
}