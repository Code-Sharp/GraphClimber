using System;
using System.Linq.Expressions;
using System.Reflection;
using GraphClimber.ExpressionCompiler;

namespace GraphClimber
{
    internal class RouteDelegateFactory
    {
        private readonly IMethodMapper _methodMapper;
        private readonly IClimbStore _climbStore;
        private readonly IExpressionCompiler _compiler;
        private readonly Type _processorType;

        public RouteDelegateFactory(Type processorType, IMethodMapper methodMapper, IClimbStore climbStore, IExpressionCompiler compiler)
        {
            _processorType = processorType;
            _methodMapper = methodMapper;
            _climbStore = climbStore;
            _compiler = compiler;
        }

        public RouteDelegate GetRouteDelegate(IStateMember member, Type runtimeMemberType)
        {
            ParameterExpression processor = Expression.Parameter(typeof (object), "processor");
            ParameterExpression owner = Expression.Parameter(typeof (object), "owner");
            ParameterExpression skipSpecialMethods = Expression.Parameter(typeof (bool), "skipSpecialMethods");

            UnaryExpression castedProcessor = 
                Expression.Convert(processor, _processorType);

            MethodInfo methodToCall =
                _methodMapper.GetMethod(_processorType, member, runtimeMemberType);

            DescriptorWriter descriptorWriter = new DescriptorWriter(_climbStore);

            Expression descriptorDeclaration = 
                descriptorWriter.WriteDescriptorDeclaration(processor, owner, member, runtimeMemberType);

            MethodCallExpression processCall =
                Expression.Call(castedProcessor,
                    methodToCall,
                    descriptorWriter.DescriptorReference);

            BlockExpression callProcess =
                Expression.Block(new[] {descriptorWriter.DescriptorReference},
                    descriptorDeclaration,
                    processCall);

            // TODO: call special methods (dough!)
            ConditionalExpression body =
                Expression.Condition(skipSpecialMethods,
                    callProcess,
                    Expression.Empty());

            Expression<RouteDelegate> lambda =
                Expression.Lambda<RouteDelegate>(body,
                    "Route_" + runtimeMemberType.Name,
                    new[] {processor, owner, skipSpecialMethods});

            return _compiler.Compile(lambda);
        }
    }
}