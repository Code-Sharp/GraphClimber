using System;

namespace GraphClimber
{
    internal class DelegateCache<TKey>
    {
        private readonly SwapDictionary<TKey, object> _cache = 
            new SwapDictionary<TKey, object>();

        protected TDelegate GetOrAdd<TDelegate>(TKey key, Func<TKey, TDelegate> delegateCreator)
        {
            TDelegate result;

            object value;

            if (_cache.TryGetValue(key, out value))
            {
                result = (TDelegate) value;
            }
            else
            {
                result = delegateCreator(key);
                _cache[key] = result;
            }

            return result;
        }

        protected TDelegate Get<TDelegate>(TKey key)
        {
            TDelegate result = default(TDelegate);

            object value;

            if (_cache.TryGetValue(key, out value))
            {
                result = (TDelegate)value;
            }

            return result;
        }
    
    }

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

    internal class ClimbDelegateCache : DelegateCache<Tuple<Type, Type>>
    {
        public new TDelegate Get<TDelegate>(Type fieldType, Type runtimeType)
        {
            return base.Get<TDelegate>(Tuple.Create(fieldType, runtimeType));
        }

        public new TDelegate GetOrAdd<TDelegate>(Type fieldType, Type runtimeType, Func<Type, Type, TDelegate> factory)
        {
            Func<Tuple<Type, Type>, TDelegate> tupleFactory =
                tuple => factory(tuple.Item1, tuple.Item2);

            return base.GetOrAdd(Tuple.Create(fieldType, runtimeType), tupleFactory);
        }
    }
}