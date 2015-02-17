using System;
using System.Linq.Expressions;
using GraphClimber.ExpressionCompiler.Extensions;

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
            return (TUnderlying) Convert.ChangeType(Get(), typeof(TUnderlying));
        }

        public void SetUnderlying(TUnderlying value)
        {
            Set((TField)Convert.ChangeType(value, typeof(TField)));
        }

        public IStateMember UnderlyingValueStateMember
        {
            get { return new EnumUnderlyingStateMember(StateMember);}
        }
    }

    internal class EnumUnderlyingStateMember : IStateMember
    {
        private readonly IStateMember _underlying;

        public EnumUnderlyingStateMember(IStateMember underlying)
        {
            _underlying = underlying;
        }

        public string Name
        {
            get { return _underlying.Name; }
        }

        public Type OwnerType
        {
            get { return _underlying.OwnerType; }
        }

        public Type MemberType
        {
            get { return _underlying.MemberType.GetEnumUnderlyingType(); }
        }

        public bool CanRead
        {
            get { return _underlying.CanRead; }
        }

        public bool CanWrite
        {
            get { return _underlying.CanWrite; }
        }

        public Expression GetGetExpression(Expression obj)
        {
            return _underlying.GetGetExpression(obj).Convert(MemberType);
        }

        public Expression GetSetExpression(Expression obj, Expression value)
        {
            return _underlying.GetSetExpression(obj, value.Convert(_underlying.MemberType));
        }

        public bool IsArrayElement
        {
            get { return _underlying.IsArrayElement; }
        }

        public int[] ElementIndex
        {
            get { return _underlying.ElementIndex; }
        }
    }


    internal class ReadWriteDescriptor<TField, TRuntime> :
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