using System;
using System.Collections.Generic;

namespace GraphClimber
{    
    internal class ReadWriteDescriptor<TField, TRuntime> :
        ValueDescriptor<TField, TRuntime>,
        IReadWriteValueDescriptor<TField>,
        IReadOnlyValueDescriptor<TRuntime>,
        IReadOnlyExactValueDescriptor<TRuntime>,
        IWriteOnlyExactValueDescriptor<TField>,
        IWriteOnlyValueDescriptor<TField>
        where TRuntime : TField
    {
        public ReadWriteDescriptor(object processor, object owner, MemberLocal<TField, TRuntime> member,
                                   IClimbStore climbStore) :
            base(processor, owner, member, climbStore)
        {
        }

        TRuntime IReadOnlyValueDescriptor<TRuntime>.Get()
        {
            return (TRuntime)Get();
        }

        TRuntime IReadOnlyExactValueDescriptor<TRuntime>.Get()
        {
            return (TRuntime)Get();
        }

        public override void Climb()
        {
            TField value = Get();
            base.Climb(value);
        }

        public TField Get()
        {
            return base.Get();
        }

        protected override void SetField(TField value)
        {
            Set(value);
        }
    }
}