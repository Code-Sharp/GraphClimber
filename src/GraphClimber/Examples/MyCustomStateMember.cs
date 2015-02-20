using System;
using System.Linq.Expressions;
using System.Reflection;

namespace GraphClimber.Examples
{
    public class MyCustomStateMember : IReflectionStateMember
    {
        private readonly IReflectionStateMember _underlying;
        private readonly Type _memberType;

        public MyCustomStateMember(IReflectionStateMember underlying, Type memberType)
        {
            _underlying = underlying;
            _memberType = memberType;
        }

        public int[] ElementIndex
        {
            get { return _underlying.ElementIndex; }
        }

        public Action<object, T> BuildSetterForBox<T>()
        {
            return _underlying.BuildSetterForBox<T>();
        }

        public bool IsArrayElement
        {
            get { return _underlying.IsArrayElement; }
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

        public Expression GetGetExpression(Expression obj)
        {
            return _underlying.GetGetExpression(obj);
        }

        public Expression GetSetExpression(Expression obj, Expression value)
        {
            return _underlying.GetSetExpression(obj, value);
        }

        public MemberInfo UnderlyingMemberInfo
        {
            get { return _underlying.UnderlyingMemberInfo; }
        }

        public object GetValue(object owner)
        {
            return _underlying.GetValue(owner);
        }

        public void SetValue(object owner, object value)
        {
            _underlying.SetValue(owner, value);
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