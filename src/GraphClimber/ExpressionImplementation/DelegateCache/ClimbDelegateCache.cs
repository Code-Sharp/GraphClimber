using System;

namespace GraphClimber
{
    internal class ClimbDelegateCache : DelegateCache<Tuple<Type, Type>>
    {
        public TDelegate Get<TDelegate>(Type fieldType, Type runtimeType)
        {
            return base.Get<TDelegate>(Tuple.Create(fieldType, runtimeType));
        }

        public TDelegate GetOrAdd<TDelegate>(Type fieldType, Type runtimeType, Func<Type, Type, TDelegate> factory)
        {
            Func<Tuple<Type, Type>, TDelegate> tupleFactory =
                tuple => factory(tuple.Item1, tuple.Item2);

            return base.GetOrAdd(Tuple.Create(fieldType, runtimeType), tupleFactory);
        }
    }
}