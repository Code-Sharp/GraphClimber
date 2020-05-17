using System;
using System.Collections.Generic;

namespace GraphClimber
{
    internal class EnumReadOnlyDescriptor<TField, TRuntime, TUnderlying> :
        ReadOnlyDescriptor<TField, TRuntime>,
        IReadOnlyEnumExactValueDescriptor<TRuntime, TUnderlying>
        where TRuntime : Enum, TField
        where TUnderlying : IConvertible
    {
        // TRuntime is Enum!
        public EnumReadOnlyDescriptor(object processor,
                                      object owner,
                                      int[] elementIndex,
                                      MemberLocal<TField, TRuntime> member,
                                      IClimbStore climbStore)
            : base(processor, owner, elementIndex, member, climbStore)
        {
        }

        public TUnderlying GetUnderlying()
        {
            TRuntime asEnum = Get();
            TUnderlying underlying = EnumConvert<TRuntime, TUnderlying>.ToUnderlying(asEnum);
            return underlying;
        }
    }

    internal class EnumWriteOnlyDescriptor<TField, TUnderlying> :
        WriteOnlyDescriptor<TField>,
        IWriteOnlyEnumExactValueDescriptor<TField, TUnderlying>
        where TUnderlying : IConvertible
        where TField : Enum
    {
        // TField is Enum!
        public EnumWriteOnlyDescriptor(object processor,
                                       object owner,
                                       int[] elementIndex,
                                       MemberLocal<TField, TField> member,
                                       IClimbStore climbStore)
            : base(processor, owner, elementIndex, member, climbStore)
        {
        }

        public void SetUnderlying(TUnderlying value)
        {
            TField @enum = EnumConvert<TField, TUnderlying>.ToEnum(value);
            Set(@enum);
        }
    }

    public interface IEnumReadWriteExactValueDescriptor<TEnum, TUnderlying> : IReadOnlyEnumExactValueDescriptor<TEnum, TUnderlying>, IWriteOnlyEnumExactValueDescriptor<TEnum, TUnderlying> 
        where TEnum : Enum, IConvertible 
        where TUnderlying : IConvertible
    {
    }

    internal class EnumReadWriteDescriptor<TField, TRuntime, TUnderlying> :
        ReadWriteDescriptor<TField, TRuntime>, 
        IEnumReadWriteExactValueDescriptor<TRuntime, TUnderlying> 
        where TRuntime : Enum, TField 
        where TUnderlying : IConvertible
    {
        // TRuntime is Enum!
        public EnumReadWriteDescriptor(object processor,
                                       object owner,
                                       int[] elementIndex,
                                       MemberLocal<TField, TRuntime> member,
                                       IClimbStore climbStore)
            : base(processor, owner, elementIndex, member, climbStore)
        {
        }

        public TUnderlying GetUnderlying()
        {
            TRuntime asEnum = (TRuntime)Get();
            TUnderlying underlying = EnumConvert<TRuntime, TUnderlying>.ToUnderlying(asEnum);
            return underlying;
        }

        public void SetUnderlying(TUnderlying value)
        {
            TRuntime @enum = EnumConvert<TRuntime, TUnderlying>.ToEnum(value);
            Set(@enum);
        }

        public void Set(TRuntime value)
        {
            base.Set(value);
        }
    }
}