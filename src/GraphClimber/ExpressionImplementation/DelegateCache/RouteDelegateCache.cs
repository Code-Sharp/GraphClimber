using System;

namespace GraphClimber
{
    internal class RouteDelegateCache : DelegateCache<Tuple<IStateMember, Type>>
    {
        public TDelegate Get<TDelegate>(IStateMember member, Type type)
        {
            return base.Get<TDelegate>(Tuple.Create(member, type));
        }

        public TDelegate GetOrAdd<TDelegate>(IStateMember member, Type type, Func<IStateMember, Type, TDelegate> factory)
        {
            Func<Tuple<IStateMember, Type>, TDelegate> tupleFactory =
                tuple => factory(tuple.Item1, tuple.Item2);

            return base.GetOrAdd(Tuple.Create(member, type), tupleFactory);
        }
    }
}