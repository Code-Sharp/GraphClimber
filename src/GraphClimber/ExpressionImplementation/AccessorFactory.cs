using System;
using System.Linq.Expressions;
namespace GraphClimber
{
    internal class AccessorFactory : IAccessorFactory
    {
        public Action<object, int[], T> GetSetter<T>(IStateMember member)
        {
            ParameterExpression instance = Expression.Parameter(typeof(object));
            ParameterExpression indices = Expression.Parameter(typeof(int[]));
            ParameterExpression value = Expression.Parameter(typeof(T));

            Expression<Action<object, int[], T>> lambda =
                Expression.Lambda<Action<object, int[], T>>
                    (member.GetSetExpression(instance, indices, value),
                        "Setter_" + member.Name,
                        new[] {instance, indices, value});

            return lambda.Compile();
        }

        public Func<object, int[], T> GetGetter<T>(IStateMember member)
        {
            ParameterExpression instance = Expression.Parameter(typeof(object));
            ParameterExpression indices = Expression.Parameter(typeof(int[]));

            Expression<Func<object, int[], T>> lambda =
                Expression.Lambda<Func<object, int[], T>>
                    (GetGetterExpression<T>(member, instance, indices),
                        "Getter_" + member.Name,
                        new[] {instance, indices});

            return lambda.Compile();
        }

        private static Expression GetGetterExpression<T>(IStateMember member, Expression instance, Expression indices)
        {
            // We need to convert to the type, since sometimes we fake the member type.
            return member.GetGetExpression(instance, indices).Convert(typeof(T));
        }
    }
}