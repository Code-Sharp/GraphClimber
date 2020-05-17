using System;

namespace GraphClimber
{
    internal interface IAccessorFactory
    {
        Action<object, int[], T> GetSetter<T>(IStateMember member);
        Func<object, int[], T> GetGetter<T>(IStateMember member);
    }
}