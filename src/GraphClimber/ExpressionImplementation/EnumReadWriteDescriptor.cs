using System;

namespace GraphClimber
{
    internal class EnumReadOnlyDescriptor<TField, TRuntime, TUnderlying> :
        ReadOnlyDescriptor<TField, TRuntime>,
        IReadOnlyEnumExactValueDescriptor<TRuntime, TUnderlying>
        where TRuntime : TField, IConvertible
        where TUnderlying : IConvertible
    {
        // TRuntime is Enum!
        public EnumReadOnlyDescriptor(object processor,
            object owner,
            MemberLocal<TField, TRuntime> member,
            IClimbStore climbStore)
            : base(processor, owner, member, climbStore)
        {
        }

        public TUnderlying GetUnderlying()
        {
            return (TUnderlying) (object) Get();
        }

        public new TField Get()
        {
            return base.Get();
        }
    }

    internal class EnumWriteOnlyDescriptor<TField, TUnderlying> :
        WriteOnlyDescriptor<TField>,
        IWriteOnlyEnumExactValueDescriptor<TField, TUnderlying>
        where TUnderlying : IConvertible
        where TField : IConvertible
    {
        // TField is Enum!
        public EnumWriteOnlyDescriptor(object processor,
            object owner,
            MemberLocal<TField, TField> member,
            IClimbStore climbStore)
            : base(processor, owner, member, climbStore)
        {
        }

        public void SetUnderlying(TUnderlying value)
        {
            Set((TField) (object) value);
        }
    }

    public interface IEnumReadWriteExactValueDescriptor<TEnum, TUnderlying> : IReadOnlyEnumExactValueDescriptor<TEnum, TUnderlying>, IWriteOnlyEnumExactValueDescriptor<TEnum, TUnderlying> 
        where TEnum : IConvertible 
        where TUnderlying : IConvertible
    {
    }

    internal class EnumReadWriteDescriptor<TField, TRuntime, TUnderlying> :
        ReadWriteDescriptor<TField, TRuntime>, 
        IEnumReadWriteExactValueDescriptor<TRuntime, TUnderlying> 
        where TRuntime : TField, IConvertible 
        where TUnderlying : IConvertible
    {
        // TRuntime is Enum!
        public EnumReadWriteDescriptor(object processor,
            object owner,
            MemberLocal<TField, TRuntime> member,
            IClimbStore climbStore)
            : base(processor, owner, member, climbStore)
        {
        }

        public TUnderlying GetUnderlying()
        {
            return (TUnderlying)(object)Get();
        }

        public void SetUnderlying(TUnderlying value)
        {
            Set((TRuntime)(object)value);
        }

        public void Set(TRuntime value)
        {
            base.Set(value);
        }
    }
}