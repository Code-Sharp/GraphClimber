using System;

namespace GraphClimber
{
    internal interface IAccessorFactory
    {
        Action<object, T> GetSetter<T>(IStateMember member);
        Func<object, T> GetGetter<T>(IStateMember member);
    }
}