using System;
using System.Linq.Expressions;
using System.Reflection;

namespace GraphClimber
{
    internal static class Method
    {
        public static MethodInfo Get<T>(Expression<Action<T>> methodCall)
        {
            MethodCallExpression callExpression = methodCall.Body as MethodCallExpression;

            return callExpression.Method;
        }
    }
}