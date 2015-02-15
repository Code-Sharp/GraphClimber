using System;
using System.Linq.Expressions;
using GraphClimber.ExpressionCompiler;
using GraphClimber.ExpressionCompiler.Extensions;

namespace GraphClimber
{
    internal class RouteDelegateFactory
    {
        private readonly IClimbStore _climbStore;
        private readonly IExpressionCompiler _compiler;
        private readonly Type _processorType;
        private readonly SpecialMethodMutator _specialMutator;
        private readonly IMethodMapper _methodMapper;

        public RouteDelegateFactory(Type processorType, IMethodMapper methodMapper, IClimbStore climbStore, IExpressionCompiler compiler)
        {
            _processorType = processorType;
            _climbStore = climbStore;
            _specialMutator = new SpecialMethodMutator(processorType);
            _methodMapper = methodMapper;
            _compiler = compiler;
        }

        public RouteDelegate GetRouteDelegate(IStateMember member, Type runtimeMemberType)
        {
            ParameterExpression processor = Expression.Parameter(typeof (object), "processor");
            ParameterExpression owner = Expression.Parameter(typeof (object), "owner");
            ParameterExpression skipSpecialMethods = Expression.Parameter(typeof (bool), "skipSpecialMethods");

            var castedProcessor =
                processor.Convert(_processorType);

            DescriptorWriter descriptorWriter = new DescriptorWriter(_climbStore);

            DescriptorVariable descriptor =
                descriptorWriter.GetDescriptor(processor, owner, member, runtimeMemberType);

            MethodInfo methodToCall =
                _methodMapper.GetMethod(_processorType, member, runtimeMemberType, true);

            MethodCallExpression callProcess =
                Expression.Call(castedProcessor, methodToCall, descriptor.Reference);

            Expression callProcessWithSpecialMethods =
                _specialMutator.Mutate(callProcess,
                    castedProcessor,
                    owner,
                    member,
                    descriptor.Reference);

            BlockExpression body =
                Expression.Block(new[] {descriptor.Reference},
                    descriptor.Declaration,
                    Expression.Condition(skipSpecialMethods,
                        callProcess,
                        callProcessWithSpecialMethods));

            Expression<RouteDelegate> lambda =
                Expression.Lambda<RouteDelegate>(body,
                    "Route_" + runtimeMemberType.Name,
                    new[] {processor, owner, skipSpecialMethods});

            return _compiler.Compile(lambda);
        }
    }
}