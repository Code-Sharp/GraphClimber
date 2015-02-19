using System;
using System.Linq.Expressions;
using System.Reflection;
using GraphClimber.ExpressionCompiler.Extensions;

namespace GraphClimber
{
    internal class NullProcessorMutator : IMethodMutator
    {
        private static readonly MethodInfo _processNull = typeof (INullProcessor).GetMethod("ProcessNull");

        private readonly bool _nullProcessorImplemented;

        public NullProcessorMutator(Type processorType)
        {
            _nullProcessorImplemented = typeof(INullProcessor).IsAssignableFrom(processorType);
        }

        public Expression Mutate(Expression oldExpression, Expression processor, Expression owner, IStateMember member, Expression descriptor)
        {
            if (!_nullProcessorImplemented)
            {
                return oldExpression;
            }

            Type memberType = member.MemberType;

            if (!member.CanRead || 
                // Currently the INullProcessor gets a IWriteOnlyValueDescriptor<T>, thats why we check
                // if the member can write.
                !member.CanWrite ||
                (memberType.IsValueType && !memberType.IsNullable()))
            {
                return oldExpression;
            }

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

            var value = member.GetGetExpression(owner);

            Expression body =
                Expression.Condition(Expression.Equal(value, ExpressionExtensions.Null),
                    Expression.Call(processor, method, descriptor),
                    oldExpression);

            return body;
        }
    }
}