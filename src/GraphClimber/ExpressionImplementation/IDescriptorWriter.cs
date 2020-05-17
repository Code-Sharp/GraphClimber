using System;
using System.Linq.Expressions;

namespace GraphClimber
{
    internal interface IDescriptorWriter
    {
        DescriptorVariable GetDescriptor
            (Expression processor,
                Expression owner,
                Expression indices,
                IStateMember member,
                Type runtimeType,
                IStateMemberProvider stateMemberProvider);
    }
}