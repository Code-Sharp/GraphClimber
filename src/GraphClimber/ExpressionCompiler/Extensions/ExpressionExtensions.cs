using System.Linq.Expressions;

namespace GraphClimber.ExpressionCompiler.Extensions
{
    /// <summary>
    /// Provides extension methods to create
    /// expressions easier.
    /// </summary>
    public static class ExpressionExtensions
    {

        /// <summary>
        /// Creates an <see cref="ConstantExpression"/>
        /// with the given value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Expression Constant<T>(this T value)
        {
            return Expression.Constant(value, typeof (T));
        }

        /// <summary>
        /// Converts the expression value to the given type
        /// <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static Expression Convert<T>(Expression expression)
        {
            return Expression.Convert(expression, typeof (T));
        }

    }
}
