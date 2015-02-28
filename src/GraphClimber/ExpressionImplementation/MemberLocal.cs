using System;

namespace GraphClimber
{
    internal class MemberLocal<TField, TRuntime>
    {
        private readonly Lazy<Func<object, TField>> _getter;
        private readonly Lazy<Action<object, TField>> _setter;
        private readonly Lazy<ClimbDelegate<TRuntime>> _climb;
        private readonly Lazy<ClimbDelegate<object>> _structClimb;
        private readonly Lazy<Func<object, TField>> _boxGetter;
        private readonly Lazy<Action<object, TField>> _boxSetter;

        private readonly IStateMember _member;

        public MemberLocal(IClimbStore climbStore,
            IStateMember member)
        {
            _member = member;
    
            _getter = 
                new Lazy<Func<object, TField>>(() => climbStore.GetGetter<TField>(member));

            _setter =
                new Lazy<Action<object, TField>>(() => climbStore.GetSetter<TField>(member));

            _boxGetter =
                new Lazy<Func<object, TField>>(() => climbStore.GetBoxGetter<TField>(member));

            _boxSetter =
                new Lazy<Action<object, TField>>(() => climbStore.GetBoxSetter<TField>(member));

            _climb =
                new Lazy<ClimbDelegate<TRuntime>>
                    (() => climbStore.GetClimb<TRuntime>(typeof(TRuntime)));

            _structClimb =
                new Lazy<ClimbDelegate<object>>
                    (() => climbStore.GetClimb<object>(typeof(TRuntime)));
        }

        public Func<object, TField> BoxGetter
        {
            get
            {
                return _boxGetter.Value;
            }
        }

        public Action<object, TField> BoxSetter
        {
            get
            {
                return _boxSetter.Value;
            }
        }

        public Func<object, TField> Getter
        {
            get
            {
                return _getter.Value;
            }
        }

        public Action<object, TField> Setter
        {
            get
            {
                return _setter.Value;
            }
        }

        public ClimbDelegate<TRuntime> Climb
        {
            get
            {
                return _climb.Value;
            }
        }

        public ClimbDelegate<object> StructClimb
        {
            get
            {
                return _structClimb.Value;
            }
        }

        public IStateMember Member
        {
            get
            {
                return _member;
            }
        }
    }
}