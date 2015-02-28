using System;

namespace GraphClimber
{
    internal interface IAccessorFactory
    {
        Action<object, T> GetSetter<T>(IStateMember member);
        Func<object, T> GetGetter<T>(IStateMember member);
        Action<object,T> GetBoxSetter<T>(IStateMember member);
        Func<object,T> GetBoxGetter<T>(IStateMember member);
    }
}