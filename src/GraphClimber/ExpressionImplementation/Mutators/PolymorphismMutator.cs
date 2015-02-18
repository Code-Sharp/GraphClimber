using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using GraphClimber.ExpressionCompiler;
using GraphClimber.ExpressionCompiler.Extensions;

namespace GraphClimber
{
    internal class PolymorphismMutator : IMethodMutator
    {
        private static readonly MethodInfo _routeMethod = typeof (IValueDescriptor)
            .GetMethods()
            .FirstOrDefault(x => x.Name == "Route" && x.GetParameters().Length == 4);

        private static readonly MethodInfo _getTypeMethod = typeof(object).GetMethod("GetType");

        public Expression Mutate(Expression oldExpression, Expression processor, Expression value, Expression owner, IStateMember member, Expression descriptor)
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