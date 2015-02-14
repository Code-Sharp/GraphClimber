using System.Collections;
using System.Collections.Generic;

namespace GraphClimber
{
    // From WampSharp
    internal class SwapDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        #region Members

        private IDictionary<TKey, TValue> _underlyingDictionary = new Dictionary<TKey, TValue>();
        private readonly object _lock = new object();

        #endregion

        #region IDictionary methods

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _underlyingDictionary.GetEnumerator();
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            lock (_lock)
            {
                IDictionary<TKey, TValue> copy = GetCopy();
                copy.Add(item);
                _underlyingDictionary = copy;
            }
        }

        private IDictionary<TKey, TValue> GetCopy()
        {
            return new Dictionary<TKey, TValue>(_underlyingDictionary);
        }

        public void Clear()
        {
            lock (_lock)
            {
                _underlyingDictionary =
                    new Dictionary<TKey, TValue>();
            }
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return _underlyingDictionary.Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            _underlyingDictionary.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            lock (_lock)
            {
                IDictionary<TKey, TValue> copy = GetCopy();
                bool result = copy.Remove(item);
                _underlyingDictionary = copy;
                return result;
            }
        }

        public int Count
        {
            get
            {
                return _underlyingDictionary.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return _underlyingDictionary.IsReadOnly;
            }
        }

        public bool ContainsKey(TKey key)
        {
            return _underlyingDictionary.ContainsKey(key);
        }

        public void Add(TKey key, TValue value)
        {
            lock (_lock)
            {
                IDictionary<TKey, TValue> copy = GetCopy();
                copy.Add(key, value);
                _underlyingDictionary = copy;
            }
        }

        public bool Remove(TKey key)
        {
            lock (_lock)
            {
                IDictionary<TKey, TValue> copy = GetCopy();
                bool result = copy.Remove(key);
                _underlyingDictionary = copy;
                return result;
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return _underlyingDictionary.TryGetValue(key, out value);
        }

        public TValue this[TKey key]
        {
            get
            {
                return _underlyingDictionary[key];
            }
            set
            {
                lock (_lock)
                {
                    IDictionary<TKey, TValue> copy = GetCopy();
                    copy[key] = value;
                    _underlyingDictionary = copy;
                }
            }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                return _underlyingDictionary.Keys;
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                return _underlyingDictionary.Values;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}