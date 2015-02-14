using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace GraphClimber.Examples
{
    public class BinaryStateMemberProvider : IStateMemberProvider
    {
        private readonly IStateMemberProvider _underlying;

        public BinaryStateMemberProvider(IStateMemberProvider underlying)
        {
            _underlying = underlying;
        }

        public IEnumerable<IStateMember> Provide(Type type)
        {
            return _underlying.Provide(type)
                .Select(x => new BinaryStateMember((IReflectionStateMember)x));
        }

        public IStateMember ProvideArrayMember(Type arrayType, int[] indices)
        {
            return new BinaryStateMember((IReflectionStateMember)
                _underlying.ProvideArrayMember(arrayType, indices));
        }
    }

    public class BinaryStateMember : IReflectionStateMember
    {
        private readonly IReflectionStateMember _stateMember;
        private readonly bool _knownType;
        private readonly bool _headerHandled;

        public BinaryStateMember(IReflectionStateMember stateMember): 
            this(stateMember, IsKnownType(stateMember), false)
        {
        }

        private static bool IsKnownType(IReflectionStateMember stateMember)
        {
            Type type = stateMember.MemberType;

            return type.IsSealed || type.IsValueType;
        }

        public BinaryStateMember(IReflectionStateMember stateMember, 
            bool knownType, 
            bool headerHandled = false)
        {
            _stateMember = stateMember;
            _knownType = knownType;
            _headerHandled = headerHandled;
        }

        public string Name
        {
            get { return _stateMember.Name; }
        }

        public Type OwnerType
        {
            get { return _stateMember.OwnerType; }
        }

        public Type MemberType
        {
            get
            {
                // A patch in order to box structs when they are objects.
                Type memberType = _stateMember.MemberType;

                if (IsObject && memberType.IsValueType && 
                    !memberType.IsPrimitive && memberType != typeof(DateTime))
                {
                    return typeof (ValueType);
                }

                return memberType;
            }
        }

        public bool CanRead
        {
            get { return _stateMember.CanRead; }
        }

        public bool CanWrite
        {
            get { return _stateMember.CanWrite; }
        }

        public Type RuntimeType
        {
            get { return _stateMember.MemberType; }
        }

        public Expression GetGetExpression(Expression obj)
        {
            return _stateMember.GetGetExpression(obj);
        }

        public Expression GetSetExpression(Expression obj, Expression value)
        {
            return _stateMember.GetSetExpression(obj, value);
        }

        public bool IsArrayElement
        {
            get { return _stateMember.IsArrayElement; }
        }

        public int[] ElementIndex
        {
            get { return _stateMember.ElementIndex; }
        }

        public bool KnownType
        {
            get { return _knownType; }
        }

        public bool IsObject
        {
            get { return HeaderHandled; }
        }

        public bool HeaderHandled
        {
            get { return _headerHandled; }
        }

        public object GetValue(object owner)
        {
            return _stateMember.GetValue(owner);
        }

        public void SetValue(object owner, object value)
        {
            _stateMember.SetValue(owner, value);
        }

        protected bool Equals(BinaryStateMember other)
        {
            return Equals(_stateMember, other._stateMember) && _knownType.Equals(other._knownType);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((BinaryStateMember) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_stateMember != null ? _stateMember.GetHashCode() : 0)*397) ^ _knownType.GetHashCode();
            }
        }
    }
}