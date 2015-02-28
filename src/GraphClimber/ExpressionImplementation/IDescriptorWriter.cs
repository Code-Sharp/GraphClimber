using System;
using System.Linq.Expressions;

namespace GraphClimber
{
    internal interface IDescriptorWriter
    {
        DescriptorVariable GetDescriptor
            (Expression processor,
                Expression owner,
                IStateMember member,
                Type runtimeType);
    }
}