using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace GraphClimber
{
    internal static class Method
    {
        public static MethodInfo Get<T>(Expression<Action<T>> methodCall)
        {
            MethodCallExpression callExpression = CastExpression<MethodCallExpression>(methodCall.Body);

            return callExpression.Method;
        }

        private static T CastExpression<T>(Expression expression, [CallerMemberName] string callerName = null) 
            where T : Expression
        {
            var retVal = expression as T;

            if (retVal == null)
            {
                throw new Exception(
                    string.Format(
                        "Method {0}.{1} expects that the method body will be expression of type {2}, got {3} instead.",
                        MethodBase.GetCurrentMethod().DeclaringType, callerName, typeof (T), expression.GetType()));
            }

            return retVal;
        }
    }
}