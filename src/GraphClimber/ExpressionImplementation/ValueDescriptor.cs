using System;

namespace GraphClimber
{
    internal abstract class ValueDescriptor<TField, TRuntime> : IValueDescriptor
        where TRuntime : TField
    {
        private readonly MemberLocal<TField, TRuntime> _member;
        private readonly IClimbStore _climbStore;
        protected readonly object _owner;
        private readonly object _processor;
        private readonly bool _isStructMember;

        protected ValueDescriptor(object processor, object owner, MemberLocal<TField, TRuntime> member, IClimbStore climbStore)
        {
            _owner = owner;
            _member = member;
            _climbStore = climbStore;
            _processor = processor;
            _isStructMember = Member.Member.OwnerType.IsValueType;
        }

        public IStateMember StateMember
        {
            get
            {
                return _member.Member;
            }
        }

        public object Owner
        {
            get
            {
                return _owner;
            }
        }

        private IClimbStore ClimbStore
        {
            get
            {
                return _climbStore;
            }
        }

        protected MemberLocal<TField, TRuntime> Member
        {
            get
            {
                return _member;
            }
        }

        protected bool IsStructMember
        {
            get
            {
                return _isStructMember;
            }
        }

        public abstract void Climb();

        protected void Climb(TField value)
        {
            if (value == null)
            {
                throw new NullReferenceException("Can't climb on null, you silly developer!");
            }

            Type type = value.GetType();

            if (type.IsValueType)
            {
                ClimbStruct(value, type);
            }
            else
            {
                ClimReference(value, type);
            }
        }

        private void ClimReference(TField value, Type type)
        {
            if (type == typeof(TRuntime))
            {
                ClimbDelegate<TRuntime> climbDelegate = Member.Climb;
                climbDelegate(_processor, (TRuntime)value);
            }
            else
            {
                ClimbDelegate<TField> climbDelegate =
                    ClimbStore.GetClimb<TField>(type);

                climbDelegate(_processor, value);
            }
        }

        private void ClimbStruct(TField value, Type type)
        {
            object boxed = value;

            ClimbDelegate<object> climbDelegate;

            if (type == typeof(TRuntime))
            {
                 climbDelegate= Member.StructClimb;
            }
            else
            {
                climbDelegate =
                    ClimbStore.GetClimb<object>(type);
            }

            climbDelegate(_processor, boxed);

            SetField((TField)boxed);
        }

        protected abstract void SetField(TField value);

        public void Route(IStateMember stateMember, Type runtimeMemberType, object owner, bool skipSpecialMethod)
        {
            RouteDelegate route = _climbStore.GetRoute(stateMember, runtimeMemberType);
            route(_processor, owner, skipSpecialMethod);
        }

        public void Route(IStateMember stateMember, object owner, bool skipSpecialMethod)
        {
            Route(stateMember, stateMember.MemberType, owner, skipSpecialMethod);
        }

        public TRuntime Get()
        {
            if (IsStructMember)
            {
                return (TRuntime)Member.BoxGetter(Owner);
            }
            else
            {
                return (TRuntime)Member.Getter(Owner);
            }
        }

        public virtual void Set(TField value)
        {
            if (IsStructMember)
            {
                Member.BoxSetter(Owner, value);
            }
            else
            {
                Member.Setter(Owner, value);                
            }
        }

        #region Enum Members

        public IStateMember UnderlyingValueStateMember
        {
            get { return new EnumUnderlyingStateMember(StateMember); }
        }

        #endregion
    }
}