using System;
using System.Linq.Expressions;

namespace GraphClimber
{
    internal class DescriptorWriter : IDescriptorWriter
    {
        private readonly IClimbStore _store;
        private ParameterExpression _descriptorReference;

        public DescriptorWriter(IClimbStore store)
        {
            _store = store;
        }

        public DescriptorVariable GetDescriptor(Expression processor, Expression owner, IStateMember member,
                                                Type runtimeType, IStateMemberProvider stateMemberProvider)
        {
            return new DescriptorVariable(_store, processor, owner, member, runtimeType, stateMemberProvider);
        }
    }
}