using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using GraphClimber.ExpressionCompiler;

namespace GraphClimber
{
    internal class ClimbDelegateFactory
    {
        private readonly Type _processorType;
        private readonly IStateMemberProvider _stateMemberProvider;
        private readonly IMethodMapper _methodMapper;
        private readonly ClimbStore _climbStore;
        private readonly IExpressionCompiler _compiler;

        public ClimbDelegateFactory(Type processorType, IStateMemberProvider stateMemberProvider, IMethodMapper methodMapper, ClimbStore climbStore, IExpressionCompiler compiler)
        {
            _processorType = processorType;
            _stateMemberProvider = stateMemberProvider;
            _methodMapper = methodMapper;
            _climbStore = climbStore;
            _compiler = compiler;
        }

        public ClimbDelegate<T> CreateDelegate<T>(Type runtimeType)
        {
            if (runtimeType.IsArray)
            {
                return CreateArrayDelegate<T>(runtimeType);
            }
            else
            {
                return CreateObjectDelegate<T>(runtimeType);
            }
        }

        private ClimbDelegate<T> CreateObjectDelegate<T>(Type runtimeType)
        {
            var processor = Expression.Parameter(typeof(object), "processor");
            var value = Expression.Parameter(typeof (T), "value");

            Expression castedProcessor = Expression.Convert(processor, _processorType);
            Expression castedValue = Expression.Convert(value, runtimeType);

            IEnumerable<IStateMember> members =
                _stateMemberProvider.Provide(runtimeType);

            List<Expression> expressions = new List<Expression>();

            foreach (IStateMember member in members)
            {
                DescriptorWriter writer = new DescriptorWriter(_climbStore);

                MethodInfo methodToCall =
                    _methodMapper.GetMethod(_processorType, member, member.MemberType);

                Expression descriptorDeclaration =
                    writer.WriteDescriptorDeclaration(castedProcessor, castedValue, member, member.MemberType);

                MethodCallExpression callProcessor =
                    Expression.Call(castedProcessor, methodToCall, writer.DescriptorReference);

                Expression expression = 
                    Expression.Block(new[] {writer.DescriptorReference},
                    descriptorDeclaration,
                    callProcessor);

                expressions.Add(expression);
                // Start without all special methods. We add them later.
            }

            BlockExpression climbBody = Expression.Block(expressions);

            Expression<ClimbDelegate<T>> lambda =
                Expression.Lambda<ClimbDelegate<T>>(climbBody,
                    "Climb_" + runtimeType.Name,
                    new[] {processor, value});

            ClimbDelegate<T> result = _compiler.Compile(lambda);

            return result;
        }

        private ClimbDelegate<T> CreateArrayDelegate<T>(Type runtimeType)
        {
            return null;
        }
    }
}