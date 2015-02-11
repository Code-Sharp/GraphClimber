using System.Linq.Expressions;

namespace GraphClimber.ExpressionCompiler
{
    /// <summary>
    /// Represents an object that can transform expressions to their describing strings.
    /// 
    /// E.g, Transform expression to C# code.
    /// </summary>
    public interface IExpressionDescriber
    {

        string Describe(Expression expression);

    }
}