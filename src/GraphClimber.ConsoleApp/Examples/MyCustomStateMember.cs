using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace GraphClimber.Examples
{
    public class MyCustomStateMember : IStateMember
    {
        private readonly IStateMember _underlying;
        private readonly Type _memberType;

        public MyCustomStateMember(IStateMember underlying, Type memberType)
        {
            _underlying = underlying;
            _memberType = memberType;
        }

        public Expression GetGetExpression(Expression obj, Expression indices)
        {
            return _underlying.GetGetExpression(obj, indices);
        }

        public Expression GetSetExpression(Expression obj, Expression indices, Expression value)
        {
            return _underlying.GetSetExpression(obj, indices, value);
        }

        public IEnumerable<string> Aliases => _underlying.Aliases;

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
            get
            {
                return _memberType;
            }
        }

        public bool CanRead
        {
            get { return _underlying.CanRead; }
        }

        public bool CanWrite
        {
            get { return _underlying.CanWrite; }
        }

        protected bool Equals(MyCustomStateMember other)
        {
            return Equals(_underlying, other._underlying) && Equals(_memberType, other._memberType);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MyCustomStateMember) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_underlying != null ? _underlying.GetHashCode() : 0)*397) ^ (_memberType != null ? _memberType.GetHashCode() : 0);
            }
        }
    }
}