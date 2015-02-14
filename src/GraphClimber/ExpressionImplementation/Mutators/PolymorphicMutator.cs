using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using GraphClimber.ExpressionCompiler.Extensions;

namespace GraphClimber
{
    internal class PolymorphicMutator : IMethodMutator
    {
        private readonly MethodInfo _routeMethod;
        private readonly MethodInfo _getTypeMethod;

        public PolymorphicMutator()
        {
            _getTypeMethod = typeof(object).GetMethod("GetType");

            _routeMethod = typeof(IValueDescriptor).GetMethods().
                FirstOrDefault(x => x.Name == "Route"
                                    && x.GetParameters().Length == 4);
        }

        public Expression Mutate(Expression oldValue, Expression processor, Expression owner, IStateMember member,
            Expression descriptor)
        {
            if (!member.CanRead)
            {
                return oldValue;
            }
            
            Type memberType = member.MemberType;
            
            if (memberType.IsValueType || memberType.IsSealed)
            {
                return oldValue;
            }

            ParameterExpression runtimeType = Expression.Variable(typeof (Type));
            
            Expression value = member.GetGetExpression(owner);

            var memberTypeConstant = Expression.Constant(memberType);

            var runtimeTypeAssign = 
                Expression.Condition(Expression.Equal(value, ExpressionExtensions.Null),
                Expression.Assign(runtimeType, memberTypeConstant),
                Expression.Assign(runtimeType, Expression.Call(value, _getTypeMethod)));

            var routeCall =
                Expression.Call(descriptor,
                    _routeMethod,
                    Expression.Constant(member),
                    runtimeType,
                    owner,
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
                            (Expression.Equal(runtimeType, Expression.Constant(member.MemberType)),
                                oldValue,
                                routeCall));

                return result;
            }
        }
    }
}