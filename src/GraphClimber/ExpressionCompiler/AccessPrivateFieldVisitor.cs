using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace GraphClimber.ExpressionCompiler
{
    public class AccessPrivateFieldVisitor : ExpressionVisitor
    {
        public static readonly ExpressionVisitor Empty = new AccessPrivateFieldVisitor();

        [DebuggerNonUserCode]
        public static object GetFieldValue(string assemblyQualifiedName, string fieldName, object instance)
        {
            return Type.GetType(assemblyQualifiedName).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).GetValue(instance);
        }

        [DebuggerNonUserCode]
        public static object GetPropertyValue(string assemblyQualifiedName, string fieldName, object instance)
        {
            return Type.GetType(assemblyQualifiedName).GetProperty(fieldName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).GetValue(instance);
        }

        [DebuggerNonUserCode]
        public static object CallMethod(string assemblyQualifiedName, string methodName, string[] genericArgumentTypes,
            object instance,
            params object[] parameters)
        {
            var methodInfo = Type.GetType(assemblyQualifiedName).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            if (methodInfo.ContainsGenericParameters)
            {
                methodInfo = methodInfo.MakeGenericMethod(genericArgumentTypes.Select(Type.GetType).ToArray());
            }

            return methodInfo.Invoke(instance, parameters);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (!node.Method.IsPublic)
            {
                var instance = node.Object ?? Expression.Constant(null);
                var genericArgumentTypes = node.Method.IsGenericMethod ? (Expression)Expression.NewArrayInit(typeof(string), node.Method.GetGenericArguments().Select(t => Expression.Constant(t.AssemblyQualifiedName))) : Expression.Empty();

                return
                    Expression.Convert(Expression.Call(null, typeof(AccessPrivateFieldVisitor).GetMethod("CallMethod"),
                        Expression.Constant(node.Method.DeclaringType.AssemblyQualifiedName),
                        Expression.Constant(node.Method.Name),
                        genericArgumentTypes,
                        instance,
                        Expression.NewArrayInit(typeof(object), node.Arguments)), node.Type);
            }

            return base.VisitMethodCall(node);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            var fieldInfo = node.Member as FieldInfo;
            if (fieldInfo != null)
            {
                FieldAttributes fieldAttributes = fieldInfo.Attributes;

                bool isPrivateField = fieldAttributes.HasFlag(FieldAttributes.Private) ||
                                      fieldAttributes.HasFlag(FieldAttributes.PrivateScope);

                if (isPrivateField)
                {
                    var argument = node.Expression ?? Expression.Constant(null);

                    return Expression.Convert(Expression.Call(null, typeof(AccessPrivateFieldVisitor).GetMethod("GetFieldValue"), Expression.Constant(fieldInfo.DeclaringType.AssemblyQualifiedName), Expression.Constant(fieldInfo.Name), argument), node.Type);
                }
            }

            var propertyInfo = node.Member as PropertyInfo;
            if (propertyInfo != null)
            {
                bool isPrivate = false;

                if (propertyInfo.CanRead)
                {
                    var getMethodAttributes = propertyInfo.GetGetMethod(true).Attributes;

                    isPrivate |= getMethodAttributes.HasFlag(MethodAttributes.Private) ||
                                 getMethodAttributes.HasFlag(MethodAttributes.PrivateScope);
                }

                if (propertyInfo.CanWrite)
                {
                    var setMethodAttributes = propertyInfo.GetSetMethod(true).Attributes;

                    isPrivate |= setMethodAttributes.HasFlag(MethodAttributes.Private) ||
                                 setMethodAttributes.HasFlag(MethodAttributes.PrivateScope);
                }

                if (isPrivate)
                {
                    var argument = node.Expression ?? Expression.Constant(null);

                    return Expression.Convert(Expression.Call(null, typeof(AccessPrivateFieldVisitor).GetMethod("GetPropertyValue"), Expression.Constant(propertyInfo.DeclaringType.AssemblyQualifiedName), Expression.Constant(propertyInfo.Name), argument), node.Type);
                }
            }


            return base.VisitMember(node);
        }
    }
}