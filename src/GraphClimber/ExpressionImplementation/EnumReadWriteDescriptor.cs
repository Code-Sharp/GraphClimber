using System;

namespace GraphClimber
{
    internal class EnumReadWriteDescriptor<TField, TRuntime, TUnderlying> :
        ReadWriteDescriptor<TField, TRuntime>,
        IReadOnlyEnumExactValueDescriptor<TField, TUnderlying>,
        IWriteOnlyEnumExactValueDescriptor<TField, TUnderlying> 
        where TRuntime : TField 
        where TField : IConvertible 
        where TUnderlying : IConvertible
    {
        public EnumReadWriteDescriptor(object processor, object owner, MemberLocal<TField, TRuntime> member, IClimbStore climbStore) : base(processor, owner, member, climbStore)
        {
        }


        public TUnderlying GetUnderlying()
        {
            return (TUnderlying) (object) Get();
        }

        public void SetUnderlying(TUnderlying value)
        {
            Set((TField) (object) value);
        }

        public IStateMember UnderlyingValueStateMember
        {
            get { return new EnumUnderlyingStateMember(StateMember);}
        }
    }
}