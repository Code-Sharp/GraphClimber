using System;
using System.Linq.Expressions;
using System.Reflection;

namespace GraphClimber
{
    internal class RevisitedMutator : IMethodMutator
    {
        private readonly MethodInfo _visitedMethod =
            typeof (IRevisitedFilter).GetMethod("Visited");

        private readonly MethodInfo _processRevisitedMethod =
            typeof (IRevisitedProcessor).GetMethod("ProcessRevisited");

        private readonly Type _processorType;

        public RevisitedMutator(Type processorType)
        {
            _processorType = processorType;
        }

        public Expression Mutate(Expression oldValue, Expression processor, Expression owner, IStateMember member,
            Expression descriptor)
        {
            if (!typeof (IRevisitedProcessor).IsAssignableFrom(_processorType))
            {
                return oldValue;
            }

            Type memberType = member.MemberType;

            if (!member.CanRead ||
                (memberType.IsValueType && !memberType.IsNullable()))
            {
                return oldValue;
            }

            Expression value = member.GetGetExpression(owner);

            MethodInfo processRevisited =
                _processRevisitedMethod.MakeGenericMethod(memberType);

            // if (processor.Visited(value)
            // {
            //      processor.ProcessRevisited(descriptor);
            // }
            // else
            // {
            //      oldValue();
            // }
            Expression body =
                Expression.Condition(Expression.Call(processor, _visitedMethod, value),
                    Expression.Call(processor, processRevisited, descriptor),
                    oldValue);

            return body;
        }
    }
}