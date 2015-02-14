using System;

namespace GraphClimber
{
    internal abstract class ValueDescriptor<TField, TRuntime> : IValueDescriptor
        where TRuntime : TField
    {
        private readonly MemberLocal<TField, TRuntime> _member;
        private readonly IClimbStore _climbStore;
        private readonly object _owner;
        private readonly object _processor;

        protected ValueDescriptor(object processor, object owner, MemberLocal<TField, TRuntime> member, IClimbStore climbStore)
        {
            _owner = owner;
            _member = member;
            _climbStore = climbStore;
            _processor = processor;
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

        public abstract void Climb();

        protected void Climb(TField value)
        {
            if (value == null)
            {
                throw new NullReferenceException("Can't climb on null, you silly developer!");
            }

            Type type = value.GetType();

            if (type == typeof(TRuntime))
            {
                ClimbDelegate<TRuntime> climbDelegate = Member.Climb;
                climbDelegate(_processor, (TRuntime) value);
            }
            else
            {
                ClimbDelegate<TField> climbDelegate = 
                    ClimbStore.GetClimb<TField>(type);

                climbDelegate(_processor, value);
            }

        }

        public void Route(IStateMember stateMember, Type runtimeMemberType, object owner, bool skipSpecialMethod)
        {
            RouteDelegate route = _climbStore.GetRoute(stateMember, runtimeMemberType);
            route(_processor, owner, skipSpecialMethod);
        }

        public void Route(IStateMember stateMember, object owner, bool skipSpecialMethod)
        {
            Route(stateMember, stateMember.MemberType, owner, skipSpecialMethod);
        }
    }
}