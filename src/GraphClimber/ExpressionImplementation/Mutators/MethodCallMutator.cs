using System;
using System.Linq.Expressions;
using System.Reflection;

namespace GraphClimber
{
    internal class MethodCallMutator : IMethodMutator
    {
        private readonly IMethodMapper _methodMapper;
        private readonly Type _processorType;

        public MethodCallMutator(Type processorType, IMethodMapper methodMapper)
        {
            _methodMapper = methodMapper;
            _processorType = processorType;
        }

        public Expression Mutate(Expression oldValue,
            Expression processor,
            Expression owner,
            IStateMember member,
            Expression descriptor)
        {
            MethodInfo methodToCall =
                _methodMapper.GetMethod(_processorType, member, member.MemberType);

            MethodCallExpression callProcessor =
                Expression.Call(processor, methodToCall, descriptor);

            Expression result = callProcessor;

            return result;
        }
    }
}