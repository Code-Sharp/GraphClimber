using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace GraphClimber
{
    internal class MemberLocal<TField, TRuntime>
    {
        private readonly Lazy<Func<object, TField>> _getter;
        private readonly Lazy<Action<object, TField>> _setter;
        private readonly Lazy<ClimbDelegate<TRuntime>> _climb;
        private readonly Lazy<ClimbDelegate<object>> _structClimb;

        private readonly IStateMember _member;
        private readonly IStateMemberProvider _provider;
        private readonly Lazy<IReadOnlyDictionary<string, IStateMember>> _children;

        public MemberLocal(IClimbStore climbStore,
                           IStateMember member,
                           IStateMemberProvider provider)
        {
            _member = member;
            _provider = provider;

            _getter =
                new Lazy<Func<object, TField>>(() => climbStore.GetGetter<TField>(member));

            _setter =
                new Lazy<Action<object, TField>>(() => climbStore.GetSetter<TField>(member));

            _climb =
                new Lazy<ClimbDelegate<TRuntime>>
                    (() => climbStore.GetClimb<TRuntime>(typeof(TRuntime)));

            _structClimb =
                new Lazy<ClimbDelegate<object>>
                    (() => climbStore.GetClimb<object>(typeof(TRuntime)));

            _children = GetChildrenLazy(typeof(TRuntime), provider);
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

        public IReadOnlyDictionary<string, IStateMember> Children { get; }

        private Lazy<IReadOnlyDictionary<string, IStateMember>> GetChildrenLazy(Type runtimeType, IStateMemberProvider memberProvider)
        {
            return new Lazy<IReadOnlyDictionary<string, IStateMember>>(CreateChildrenDictionary);

            IReadOnlyDictionary<string, IStateMember> CreateChildrenDictionary() =>
                new ReadOnlyDictionary<string, IStateMember>(GetChildren()
                                                                 .ToDictionary(x => x.alias, x => x.stateMember));

            IEnumerable<(string alias, IStateMember stateMember)> GetChildren()
            {
                foreach (IStateMember stateMember in memberProvider.Provide(runtimeType))
                {
                    foreach (string alias in stateMember.Aliases ?? Enumerable.Empty<string>())
                    {
                        yield return (alias, stateMember);
                    }
                }
            }
        }
    }
}