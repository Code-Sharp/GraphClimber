using System;
using System.Collections.Generic;

namespace GraphClimber
{
    internal class WriteOnlyDescriptor<TField> : ValueDescriptor<TField, TField>, IWriteOnlyValueDescriptor<TField>,
        IWriteOnlyExactValueDescriptor<TField>
    {
        private bool _setCalled;
        private TField _newValue;

        public WriteOnlyDescriptor(object processor, object owner, MemberLocal<TField, TField> member,
                                   IClimbStore climbStore)
            : base(processor, owner, member, climbStore)
        {
        }

        public override void Climb()
        {
            if (!_setCalled)
            {
                throw new Exception("This is a write-only descriptor. Calling Set() is required before calling Climb().");
            }

            base.Climb(_newValue);
        }

        public override void Set(TField value)
        {
            _setCalled = true;
            _newValue = value;
            base.Set(value);
        }

        protected override void SetField(TField value)
        {
            Set(value);
        }
    }
}