using System;
using System.Collections.Generic;

namespace GraphClimber
{
    internal class ReadOnlyDescriptor<TField, TRuntime> :
        ValueDescriptor<TField, TRuntime>,
        IReadOnlyValueDescriptor<TRuntime>,
        IReadOnlyExactValueDescriptor<TRuntime>
        where TRuntime : TField
    {
        public ReadOnlyDescriptor(object processor, object owner, MemberLocal<TField, TRuntime> member, IClimbStore climbStore)
            : base(processor, owner, member, climbStore)
        {
        }

        public TRuntime Get()
        {
            return (TRuntime)Member.Getter(Owner);
        }

        public override void Climb()
        {
            // TODO: Maybe this value should be passed via ctor.
            TRuntime value = Get();

            Climb(value);
        }

        protected override void SetField(TField value)
        {
            TField field = Get();

            if (!EqualityComparer<TField>.Default.Equals(field, value))
            {
                throw new System.NotImplementedException("Are you crazy???");
            }
        }
    }
}