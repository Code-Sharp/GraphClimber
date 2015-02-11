using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace GraphClimber.ExpressionCompiler
{
    public interface IExpressionCompiler
    {

        TDelegate Compile<TDelegate>(Expression<TDelegate> expression);

    }

    public class TrivialExpressionCompiler : IExpressionCompiler
    {
        public TDelegate Compile<TDelegate>(Expression<TDelegate> expression)
        {
            return expression.Compile();
        }
    }

    public interface IExpressionDescriber
    {

        string Describe(Expression expression);

    }

    public class DebugViewExpressionDescriber : IExpressionDescriber
    {
        public static readonly IExpressionDescriber Empty = new DebugViewExpressionDescriber();

        private static readonly PropertyInfo _debugView = typeof(Expression).GetProperty("DebugView", BindingFlags.NonPublic | BindingFlags.Instance);

        private DebugViewExpressionDescriber()
        {
            
        }

        private static string GetDebugView(Expression expression)
        {
            return (string) _debugView.GetValue(expression);
        }

        public string Describe(Expression expression)
        {
            return GetDebugView(expression);
        }
    }

    public class DebugExpressionCompiler : IExpressionCompiler
    {
        private readonly IExpressionDescriber _expressionDescriber;

        public DebugExpressionCompiler(IExpressionDescriber expressionDescriber)
        {
            _expressionDescriber = expressionDescriber;
        }

        public TDelegate Compile<TDelegate>(Expression<TDelegate> expression)
        {
            var dynamicType = GetDynamicTypeBuilder();
            var dynamicMethod = dynamicType.DefineMethod(expression.Name, MethodAttributes.Public | MethodAttributes.Static);

            var tempFileName = Path.GetTempFileName();

            File.AppendAllText(tempFileName, _expressionDescriber.Describe(expression));

            var symbolDocument = Expression.SymbolDocument(tempFileName);

            var finalExpression = MutateLambdaExpression(expression, symbolDocument);

            CompileToMethod(finalExpression, dynamicMethod);

            var newType = dynamicType.CreateType();

            return (TDelegate)(object)Delegate.CreateDelegate(typeof(TDelegate), newType.GetMethod(dynamicMethod.Name));
        }

        private static void CompileToMethod<TDelegate>(Expression<TDelegate> expression,
            MethodBuilder dynamicMethod)
        {
            var gen = DebugInfoGenerator.CreatePdbGenerator();
            expression.CompileToMethod(dynamicMethod, gen);
        }

        private Expression<TDelegate> MutateLambdaExpression<TDelegate>(Expression<TDelegate> expression,
            SymbolDocumentInfo symbolDocument)
        {
            Expression returnValue = expression;

            foreach (var visitor in GetVisitors(expression, symbolDocument))
            {
                returnValue = visitor.Visit(returnValue);
            }

            return (Expression<TDelegate>)returnValue;
        }

        private static TypeBuilder GetDynamicTypeBuilder()
        {
            var dynamicAssembly = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName("DynamicAssembly"),
                AssemblyBuilderAccess.RunAndSave);
            var dynamicModule = dynamicAssembly.DefineDynamicModule("DynamicModule", true);
            var dynamicType = dynamicModule.DefineType("GeneratedType", TypeAttributes.Public);
            return dynamicType;
        }

        private ExpressionVisitor[] GetVisitors(Expression expression, SymbolDocumentInfo symbolDocument)
        {
            return new[] { new DebugExpressionVistor(symbolDocument, expression, _expressionDescriber), AccessPrivateFieldVisitor.Empty };
        }
    }

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

    public static class StringExtensions
    {

        public static Position GetPosition(this string str, int index)
        {
            var column = index;
            var newLineIndex = str.IndexOf(Environment.NewLine, StringComparison.Ordinal);
            var newLines = 0;

            while (newLineIndex != -1 && newLineIndex < index)
            {
                newLines++;
                column = index - newLineIndex - Environment.NewLine.Length;
                newLineIndex = str.IndexOf(Environment.NewLine, newLineIndex + 1, StringComparison.Ordinal);
            }


            return new Position { Line = newLines, Column = column };
        }

        public struct Position
        {
            public Position(int line, int column)
                : this()
            {
                Line = line;
                Column = column;
            }

            public int Line { get; set; }

            public int Column { get; set; }

        }
    }

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
