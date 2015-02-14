using System;
using System.Linq.Expressions;
using System.Reflection;
using GraphClimber.ExpressionCompiler.Extensions;

namespace GraphClimber
{
    internal class NullProcessorMutator : IMethodMutator
    {
        private readonly Type _processorType;

        private readonly MethodInfo _processNull = typeof (INullProcessor).GetMethod("ProcessNull");

        public NullProcessorMutator(Type processorType)
        {
            _processorType = processorType;
        }

        public Expression Mutate(Expression oldValue, Expression processor, Expression owner, IStateMember member,
            Expression descriptor)
        {
            if (!typeof(INullProcessor).IsAssignableFrom(_processorType))
            {
                return oldValue;
            }

            Type memberType = member.MemberType;

            if (!member.CanRead || 
                // Currently the INullProcessor gets a IWriteOnlyValueDescriptor<T>, thats why we check
                // if the member can write.
                !member.CanWrite ||
                (memberType.IsValueType && !memberType.IsNullable()))
            {
                return oldValue;
            }

            Expression value = member.GetGetExpression(owner);

            MethodInfo method = 
                _processNull.MakeGenericMethod(member.MemberType);

            // if (value == null)
            // {
            //      processor.ProcessNull<TField>(descriptor);
            // }
            // else
            // {
            //      oldExpression();
            // }

            Expression body =
                Expression.Condition(Expression.Equal(value, ExpressionExtensions.Null),
                    Expression.Call(processor, method, descriptor),
                    oldValue);

            return body;
        }
    }
}