using System;
using System.Reflection;

namespace GraphClimber
{
    internal interface IMethodMapper
    {
        MethodInfo GetMethod(Type processorType, IStateMember member, Type runtimeType);
    }
}