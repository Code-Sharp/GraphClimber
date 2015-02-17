using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace GraphClimber.ExpressionCompiler
{
    public class DebugExpressionVistor : ExpressionVisitor
    {
        private static readonly ICollection<Type> _untouchedExpressionTypes  = new[] { typeof(ParameterExpression), typeof(ConstantExpression) };
        private static readonly ICollection<Type> _unsymboledExpressionTypes = new[] { typeof(BlockExpression), typeof(LambdaExpression), typeof(LoopExpression) };

        private static readonly string[] _newLineSeperator = { Environment.NewLine };

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

        private bool ContainsType(IEnumerable<Type> types, object instance)
        {
            var type = instance.GetType();

            return types.Any(currentType => currentType.IsAssignableFrom(type));
        }

        public override Expression Visit(Expression node)
        {
            if (node == null)
            {
                return null;
            }

            // Untouched remains as they are
            if (ContainsType(_untouchedExpressionTypes, node))
            {
                return node;
            }

            // Unsymboled is not symboled, the node children are.
            if (ContainsType(_unsymboledExpressionTypes, node))
            {
                return base.Visit(node);
            }

            Range<Position> range = GetCurrentExpressionRange(node);

            // Still not sure why I have so many plus one.
            Expression debugInfoExpression = Expression.DebugInfo(_symbolDocumentInfo, 
                range.Start.Line + 1, 
                range.Start.Column + 1, 
                range.End.Line + 1, 
                range.End.Column + 1);

            Expression innerExpression = base.Visit(node);

            return Expression.Block(debugInfoExpression, innerExpression);
        }

        private Range<Position> GetCurrentExpressionRange(Expression node)
        {
            string debugView = _expressionDescriber.Describe(node);
            string[] debugViewLines = debugView.Split(_newLineSeperator, StringSplitOptions.RemoveEmptyEntries);
            string firstDebugViewLine = debugViewLines.First();

            var newIndex  = _initialDebugView.IndexOf(firstDebugViewLine, _currentIndex, StringComparison.Ordinal);
            _currentIndex = newIndex;

            Position start = _initialDebugView.GetPosition(_currentIndex);
            int spacing = _initialDebugView.Split(_newLineSeperator, StringSplitOptions.None)[start.Line].IndexOfNot(' ');

            Position end = _initialDebugView.GetPosition(_currentIndex + debugView.Length + (debugViewLines.Length - 1) * spacing);

            return new Range<Position>(start, end);
        }

        private struct Range<T>
        {
            private readonly T _start;
            private readonly T _end;

            public Range(T start, T end)
            {
                _start = start;
                _end = end;
            }

            public T Start
            {
                get { return _start; }
            }

            public T End
            {
                get { return _end; }
            }
        }
    }
}