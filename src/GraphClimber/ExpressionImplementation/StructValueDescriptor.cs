namespace GraphClimber
{
    internal class Box<T>
    {
        public Box(T value)
        {
            Value = value;
        }

        public T Value;
    }

    internal abstract class StructValueDescriptor<TStruct, TField, TRuntime> : ValueDescriptor<TField, TRuntime, Box<TStruct>>
        where TRuntime : TField 
        where TStruct : struct
    {
        protected StructValueDescriptor(object processor, Box<TStruct> owner, MemberLocal<TField, TRuntime> member, IClimbStore climbStore) :
            base(processor, owner, member, climbStore)
        {
        }

        public override object Owner
        {
            get
            {
                // was once return _owner.Value, but that ain't good enough.
                // because the user won't be able to call route himself.
                return _owner;
            }
        }
    }

    internal class StructReadWriteDescriptor<TStruct ,TField, TRuntime> :
        StructValueDescriptor<TStruct, TField, TRuntime>,
        IReadWriteValueDescriptor<TField>,
        IReadOnlyValueDescriptor<TRuntime>,
        IReadOnlyExactValueDescriptor<TRuntime>,
        IWriteOnlyExactValueDescriptor<TField>,
        IWriteOnlyValueDescriptor<TField>
        where TRuntime : TField 
        where TStruct : struct
    {
        private bool _setCalled = false;

        public StructReadWriteDescriptor(object processor,
            Box<TStruct> owner,
            MemberLocal<TField, TRuntime> member,
            IClimbStore climbStore) :
                base(processor, owner, member, climbStore)
        {
        }

        public void Set(TField value)
        {
            _setCalled = true;
            Member.BoxSetter(_owner, value);
        }

        public TField Get()
        {
            return Member.BoxGetter(_owner);
        }

        TRuntime IReadOnlyValueDescriptor<TRuntime>.Get()
        {
            return (TRuntime) Get();
        }

        TRuntime IReadOnlyExactValueDescriptor<TRuntime>.Get()
        {
            return (TRuntime) Get();
        }

        public override void Climb()
        {
            TField value = Get();
            base.Climb(value);
        }

        protected override void SetField(TField value)
        {
            Set(value);
        }
    }
}