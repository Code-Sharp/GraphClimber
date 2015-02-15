using System;
using System.Linq.Expressions;
using GraphClimber.ExpressionCompiler;
using GraphClimber.ExpressionCompiler.Extensions;

namespace GraphClimber
{
    internal class AccessorFactory : IAccessorFactory
    {
        private readonly IExpressionCompiler _compiler;

        public AccessorFactory(IExpressionCompiler compiler)
        {
            _compiler = compiler;
        }

        public Action<object, T> GetSetter<T>(IStateMember member)
        {
            ParameterExpression instance = Expression.Parameter(typeof(object));
            ParameterExpression value = Expression.Parameter(typeof(T));

            Expression<Action<object, T>> lambda =
                Expression.Lambda<Action<object, T>>
                    (member.GetSetExpression(instance, value),
                        "Setter_" + member.Name,
                        new[] {instance, value});

            return _compiler.Compile(lambda);
        }

        public Func<object, T> GetGetter<T>(IStateMember member)
        {
            ParameterExpression instance = Expression.Parameter(typeof(object));

            Expression<Func<object, T>> lambda =
                Expression.Lambda<Func<object, T>>
                    (
                        // We need to convert to the type, since sometimes we fake the member type.
                        member.GetGetExpression(instance).Convert(typeof (T)),
                        "Getter_" + member.Name,
                        new[] {instance});

            return _compiler.Compile(lambda);
        }
    }
}