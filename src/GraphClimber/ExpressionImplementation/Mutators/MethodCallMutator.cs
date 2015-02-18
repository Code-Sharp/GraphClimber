using System;
using System.Linq.Expressions;
using System.Reflection;

namespace GraphClimber
{
    internal class MethodCallMutator : IMethodMutator
    {
        private readonly IMethodMapper _methodMapper;
        private readonly bool _routed;
        private readonly Type _processorType;

        public MethodCallMutator(Type processorType, IMethodMapper methodMapper, bool routed)
        {
            _methodMapper = methodMapper;
            _routed = routed;
            _processorType = processorType;
        }

        public Expression Mutate(Expression oldExpression, Expression processor, Expression value, Expression owner, IStateMember member, Expression descriptor)
        {
            MethodInfo methodToCall =
                _methodMapper.GetMethod(_processorType, member, member.MemberType, _routed);

            MethodCallExpression callProcessor =
                Expression.Call(processor, methodToCall, descriptor);

            Expression result = callProcessor;

            return result;
        }
    }
}