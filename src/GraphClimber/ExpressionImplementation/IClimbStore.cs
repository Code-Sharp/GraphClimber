using System;

namespace GraphClimber
{
    internal interface IClimbStore
    {
        Action<object, int[], T> GetSetter<T>(IStateMember member);
        Func<object, int[], T> GetGetter<T>(IStateMember member);
        RouteDelegate GetRoute(IStateMember member, Type runtimeMemberType);
        ClimbDelegate<T> GetClimb<T>(Type runtimeType);
    }
}