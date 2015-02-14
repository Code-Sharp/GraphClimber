using System;

namespace GraphClimber
{
    internal class AccessorDelegateCache : DelegateCache<IStateMember>
    {
        public new TDelegate Get<TDelegate>(IStateMember member)
        {
            return base.Get<TDelegate>(member);
        }

        public new TDelegate GetOrAdd<TDelegate>(IStateMember member, Func<IStateMember, TDelegate> factory)
        {
            return base.GetOrAdd(member, factory);
        }
    }
}