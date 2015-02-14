using System;
using GraphClimber.ExpressionCompiler;

namespace GraphClimber
{
    internal class ClimbStore : IClimbStore
    {
        private readonly AccessorDelegateCache _setters = new AccessorDelegateCache();
        private readonly AccessorDelegateCache _getters = new AccessorDelegateCache();
        private readonly ClimbDelegateCache _climbs = new ClimbDelegateCache();
        private readonly RouteDelegateCache _routes = new RouteDelegateCache();

        private readonly IAccessorFactory _accessorFactory;
        private readonly RouteDelegateFactory _routeFactory;
        private readonly ClimbDelegateFactory _climbFactory;

        public ClimbStore(Type processorType, IStateMemberProvider stateMemberProvider, IMethodMapper methodMapper, IExpressionCompiler compiler)
        {
            _accessorFactory = new AccessorFactory(compiler);
            _routeFactory = new RouteDelegateFactory(processorType, methodMapper, this, compiler);
            _climbFactory = new ClimbDelegateFactory(processorType, stateMemberProvider, methodMapper, this, compiler);
        }

        public Action<object, T> GetSetter<T>(IStateMember member)
        {
            return _setters.GetOrAdd
                (member,
                key => _accessorFactory.GetSetter<T>(key));
        }

        public Func<object, T> GetGetter<T>(IStateMember member)
        {
            return _getters.GetOrAdd
                (member,
                key => _accessorFactory.GetGetter<T>(key));
        }

        public RouteDelegate GetRoute(IStateMember member, Type runtimeMemberType)
        {
            return _routes.GetOrAdd(member, runtimeMemberType,
                (memberKey, runtimeTypeKey) =>
                    _routeFactory.GetRouteDelegate(memberKey, runtimeTypeKey));
        }

        public ClimbDelegate<TField> GetClimb<TField>(Type runtimeType)
        {
            return _climbs.GetOrAdd(typeof (TField), runtimeType,
                (typeKey, runtimeTypeKey) => 
                    _climbFactory.CreateDelegate<TField>(runtimeTypeKey));
        }
    }
}