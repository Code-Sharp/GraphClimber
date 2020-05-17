using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace GraphClimber
{
    internal class EnumUnderlyingStateMember<TEnum> : IStateMember
    {
        private readonly IStateMember _underlying;

        public EnumUnderlyingStateMember(IStateMember underlying)
        {
            _underlying = underlying;
        }

        public string Name
        {
            get { return _underlying.Name; }
        }

        public Type OwnerType
        {
            get { return _underlying.OwnerType; }
        }

        public Type MemberType
        {
            get { return typeof(TEnum).GetEnumUnderlyingType(); }
        }

        public bool CanRead
        {
            get { return _underlying.CanRead; }
        }

        public bool CanWrite
        {
            get { return _underlying.CanWrite; }
        }

        public Expression GetGetExpression(Expression obj, Expression indices)
        {
            return _underlying.GetGetExpression(obj, indices).Convert(MemberType);
        }

        public Expression GetSetExpression(Expression obj, Expression indices, Expression value)
        {
            return _underlying.GetSetExpression(obj, indices, value.Convert(_underlying.MemberType));
        }

        public IEnumerable<string> Aliases
        {
            get
            {
                yield return Name;
            }
        }

        protected bool Equals(EnumUnderlyingStateMember<TEnum> other)
        {
            return Equals(_underlying, other._underlying);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((EnumUnderlyingStateMember<TEnum>)obj);
        }

        public override int GetHashCode()
        {
            return (_underlying != null ? _underlying.GetHashCode() : 0);
        }
    }
}