using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
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
                    (GetGetterExpression<T>(member, instance),
                        "Getter_" + member.Name,
                        new[] {instance});

            return _compiler.Compile(lambda);
        }

        private static Expression GetGetterExpression<T>(IStateMember member, Expression instance)
        {
            // We need to convert to the type, since sometimes we fake the member type.
            return member.GetGetExpression(instance).Convert(typeof(T));
        }

        public Action<object, T> GetBoxSetter<T>(IStateMember member)
        {
            // TODO: this should be somewhere specific to the state member.
            IReflectionStateMember stateMember = member as IReflectionStateMember;
            PropertyInfo property = stateMember.UnderlyingMemberInfo as PropertyInfo;

            // See http://stackoverflow.com/questions/18937935/how-to-mutate-a-boxed-struct-using-il
            var dynamicMethod =
                new DynamicMethod("BoxSetter_" + member.Name, typeof (void), new[] {typeof (object), typeof (T)},
                    typeof (AccessorFactory).Module, true);
            
            var generator = dynamicMethod.GetILGenerator();
            generator.Emit(OpCodes.Ldarg_0);                       // object
            generator.Emit(OpCodes.Unbox, member.OwnerType);  // Struct&
            generator.Emit(OpCodes.Ldarg_1);                       // Struct& T
            generator.Emit(OpCodes.Call, property.SetMethod);                  // --empty--
            generator.Emit(OpCodes.Ret);                           // --empty--

            Action<object, T> result = (Action<object, T>) dynamicMethod.CreateDelegate(typeof (Action<object, T>));

            return result;
        }

        public Func<object, T> GetBoxGetter<T>(IStateMember member)
        {
            return GetGetter<T>(member);
        }
    }
}