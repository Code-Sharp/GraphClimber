using System;
using System.Linq.Expressions;
using System.Reflection;

namespace GraphClimber
{
    internal class PolymorphismMutator : IMethodMutator
    {
        private static readonly MethodInfo _routeMethod =
            Method.Get((IValueDescriptor descriptor) =>
                descriptor.Route(default(IStateMember), default(Type), default(object), default(bool)));

        private static readonly MethodInfo _getTypeMethod = 
            Method.Get((object instance) => instance.GetType());

        public Expression Mutate(Expression oldExpression, Expression processor, Expression owner, IStateMember member,
                                 Expression descriptor, Expression indices)
        {
            if (!member.CanRead)
            {
                return oldExpression;
            }
            
            Type memberType = member.MemberType;
            
            if (memberType.IsValueType || memberType.IsSealed)
            {
                return oldExpression;
            }

            ParameterExpression runtimeType = Expression.Variable(typeof (Type), member.Name.FirstLowerCase() + "RuntimeType");
            
            var memberTypeConstant = memberType.Constant();

            var value = member.GetGetExpression(owner, indices);

            var runtimeTypeAssign = 
                Expression.Condition(Expression.Equal(value, ExpressionExtensions.Null),
                Expression.Assign(runtimeType, memberTypeConstant),
                Expression.Assign(runtimeType, Expression.Call(value, _getTypeMethod)));

            var routeCall =
                Expression.Call(descriptor,
                    _routeMethod,
                    Expression.Constant(member),
                    runtimeType,
                    owner.Convert<object>(),
                    Expression.Constant(true));

            if (memberType.IsAbstract || memberType.IsInterface)
            {
                // Replace the call with a route in this case.
                BlockExpression block =
                    Expression.Block(new[] {runtimeType},
                        runtimeTypeAssign,
                        routeCall);

                return block;
            }
            else
            {
                // The code that should be generated is this :

                // Type runtimeType;
                // if (value != null)
                // {
                //       runtimeType = value.GetType();
                // }
                // else
                // {
                //      runtimeType = memberType;
                // }
                //
                // if (runtimeType == memberType)
                // {
                //      oldExpression();
                // }
                // else
                // {
                //      descriptor.Route(member, runtimeType, owner, true);
                // }
                // 

                Expression result =
                    Expression.Block(new[] {runtimeType},
                        runtimeTypeAssign,
                        Expression.Condition
                            (Expression.Equal(runtimeType, member.MemberType.Constant()),
                                oldExpression,
                                routeCall));

                return result;
            }
        }
    }
}