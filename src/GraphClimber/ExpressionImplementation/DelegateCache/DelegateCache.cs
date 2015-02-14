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
}