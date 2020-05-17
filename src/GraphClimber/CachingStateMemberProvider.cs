using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using GraphClimber.ValueDescriptor;

namespace GraphClimber
{
    public class CachingStateMemberProvider : IStateMemberProvider
    {
        private readonly IStateMemberProvider _underlying;
        
        private readonly IDictionary<Type, IEnumerable<IStateMember>> _cache = new Dictionary<Type, IEnumerable<IStateMember>>();
        private readonly object _syncRoot = new object();

        public CachingStateMemberProvider(IStateMemberProvider underlying)
        {
            _underlying = underlying;
        }

        public IEnumerable<IStateMember> Provide(Type type)
        {
            IEnumerable<IStateMember> retVal;
            if (_cache.TryGetValue(type, out retVal))
            {
                return retVal;
            }

            lock (_syncRoot)
            {
                if (!_cache.TryGetValue(type, out retVal))
                {
                    // ToList() is here to cache yield return methods.
                    _cache[type] = retVal = _underlying.Provide(type).ToList();
                }
            }

            return retVal;
        }

        public IStateMember ProvideArrayMember(Type arrayType, int arrayLength)
        {
            return _underlying.ProvideArrayMember(arrayType, arrayLength);
        }
    }
}