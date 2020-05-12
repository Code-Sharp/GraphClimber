using System;
using System.Linq.Expressions;

namespace GraphClimber
{
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

            var unaryExpression = expression as UnaryExpression;
            if (unaryExpression != null)
            {
                if (unaryExpression.NodeType == ExpressionType.Convert &&
                    !unaryExpression.IsCustomConversion())
                {
                    return Convert(unaryExpression.Operand, newType);
                }
            }

            return Expression.Convert(expression, newType);
        }

        private static bool IsCustomConversion(this UnaryExpression unaryExpression)
        {
            Type conversionType = unaryExpression.Type;
            Type originalType = unaryExpression.Operand.Type;

            return !(conversionType.IsAssignableFrom(originalType) ||
                     originalType.IsAssignableFrom(conversionType));
        }

        public static string FirstLowerCase(this string str)
        {
            return str.Substring(0, 1).ToLowerInvariant() + str.Substring(1);
        }
    }
}