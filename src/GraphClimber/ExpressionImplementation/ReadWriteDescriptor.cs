namespace GraphClimber
{
    internal class ReadWriteDescriptor<TRuntime, TField> :
        ValueDescriptor<TField, TRuntime>,
        IReadWriteValueDescriptor<TField>,
        IReadOnlyValueDescriptor<TRuntime>,
        IReadOnlyExactValueDescriptor<TRuntime>,
        IWriteOnlyExactValueDescriptor<TField>,
        IWriteOnlyValueDescriptor<TField>
        where TRuntime : TField
    {
        private bool _setCalled = false;

        public ReadWriteDescriptor(object processor, object owner, MemberLocal<TField, TRuntime> member, IClimbStore climbStore) :
            base(processor, owner, member, climbStore)
        {
        }

        public void Set(TField value)
        {
            _setCalled = true;
            Member.Setter(this.Owner, value);
        }

        public TField Get()
        {
            return Member.Getter(this.Owner);
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
    }
}