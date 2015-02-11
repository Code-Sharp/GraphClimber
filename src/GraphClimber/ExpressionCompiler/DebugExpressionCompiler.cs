using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace GraphClimber.ExpressionCompiler
{
    /// <summary>
    /// An <see cref="IExpressionCompiler"/>
    /// That compiles methods into dynamic assemblies.
    /// 
    /// Also it, Writes the expression description to temporary file and allows
    /// debugging with normal IDE (e.g Visual Studio).
    /// </summary>
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
            Expression returnValue = GetVisitors(expression, symbolDocument)
                .Aggregate<ExpressionVisitor, Expression>(expression, (current, visitor) => visitor.Visit(current));

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
}