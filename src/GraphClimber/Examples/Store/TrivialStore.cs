using System.Collections.Generic;

namespace GraphClimber.Examples
{
    public class TrivialStore : IStore
    {
        private readonly IDictionary<string, object> _store = new Dictionary<string, object>();

        public IStore GetInner(string path)
        {
            return new InnerStore(this, path);
        }

        public void Set<T>(string path, T value)
        {
            _store[path] = value;
        }

        public bool TryGet<T>(string path, out T value)
        {
            object boxedValue;
            if (_store.TryGetValue(path, out boxedValue))
            {
                value = (T) boxedValue;
                return true;
            }
            value = default(T);
            return false;
        }

        private class InnerStore : IStore
        {
            private readonly IStore _store;
            private readonly string _path;

            public InnerStore(IStore store, string path)
            {
                _store = store;
                _path = path;
            }

            public IStore GetInner(string path)
            {
                return new InnerStore(_store, GetPath(path));
            }

            public void Set<T>(string path, T value)
            {
                _store.Set(GetPath(path), value);
            }

            public bool TryGet<T>(string path, out T value)
            {
                return _store.TryGet(GetPath(path), out value);
            }

            private string GetPath(string path)
            {
                return _path + "." + path;
            }
        }

    }
}