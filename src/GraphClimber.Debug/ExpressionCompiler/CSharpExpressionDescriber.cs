using System.IO;
using System.Linq.Expressions;
using GraphClimber.ExpressionCompiler;
using Mono.Linq.Expressions;

namespace GraphClimber.Debug.ExpressionCompiler
{
    public class CSharpExpressionDescriber : IExpressionDescriber
    {
        public static readonly IExpressionDescriber Empty = new CSharpExpressionDescriber();

        private CSharpExpressionDescriber()
        {
            
        }

        public string Describe(Expression expression)
        {
            var stringWriter = new StringWriter();
            var csharpWriter = new CSharpWriter(new TextFormatter(stringWriter));

            csharpWriter.Write(expression);

            return stringWriter.ToString();
        }
    }
}