using System;
using System.Linq.Expressions;
namespace GraphClimber
{
    internal class AccessorFactory : IAccessorFactory
    {
        public Action<object, T> GetSetter<T>(IStateMember member)
        {
            ParameterExpression instance = Expression.Parameter(typeof(object));
            ParameterExpression value = Expression.Parameter(typeof(T));

            Expression<Action<object, T>> lambda =
                Expression.Lambda<Action<object, T>>
                    (member.GetSetExpression(instance, value),
                        "Setter_" + member.Name,
                        new[] {instance, value});

            return lambda.Compile();
        }

        public Func<object, T> GetGetter<T>(IStateMember member)
        {
            ParameterExpression instance = Expression.Parameter(typeof(object));

            Expression<Func<object, T>> lambda =
                Expression.Lambda<Func<object, T>>
                    (GetGetterExpression<T>(member, instance),
                        "Getter_" + member.Name,
                        new[] {instance});

            return lambda.Compile();
        }

        private static Expression GetGetterExpression<T>(IStateMember member, Expression instance)
        {
            // We need to convert to the type, since sometimes we fake the member type.
            return member.GetGetExpression(instance).Convert(typeof(T));
        }
    }
}