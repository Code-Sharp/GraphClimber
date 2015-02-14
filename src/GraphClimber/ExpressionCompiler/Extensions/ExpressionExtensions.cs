using System;
using System.Linq.Expressions;

namespace GraphClimber.ExpressionCompiler.Extensions
{
    /// <summary>
    /// Provides extension methods to create
    /// expressions easier.
    /// </summary>
    internal static class ExpressionExtensions
    {

        /// <summary>
        /// Expression of Null.
        /// </summary>
        public static readonly ConstantExpression Null = Expression.Constant(null);
        
        /// <summary>
        /// Empty Expression.
        /// </summary>
        public static readonly DefaultExpression Empty = Expression.Empty();

        /// <summary>
        /// Creates an <see cref="ConstantExpression"/>
        /// with the given value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Expression Constant<T>(this T value)
        {
            return Expression.Constant(value);
        }

        /// <summary>
        /// Converts the expression value to the given type
        /// <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static Expression Convert<T>(this Expression expression)
        {
            return expression.Convert(typeof (T));
        }

        /// <summary>
        /// Converts the expression value to the given
        /// <param name="newType"></param>
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="newType"></param>
        /// <returns></returns>
        public static Expression Convert(this Expression expression, Type newType)
        {
            // Avoid redundent converts
            if (expression.Type == newType)
            {
                return expression;
            }

            return Expression.Convert(expression, newType);
        }

    }
}
