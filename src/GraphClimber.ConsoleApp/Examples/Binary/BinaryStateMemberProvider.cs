using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

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
                .Select(x => new BinaryStateMember(x));
        }

        public IStateMember ProvideArrayMember(Type arrayType, int arrayLength)
        {
            return new BinaryStateMember(_underlying.ProvideArrayMember(arrayType, arrayLength));
        }
    }

    public class BinaryStateMember : IStateMember
    {
        private readonly IStateMember _stateMember;
        private readonly bool _knownType;
        private readonly bool _headerHandled;

        public BinaryStateMember(IStateMember stateMember): 
            this(stateMember, IsKnownType(stateMember), false)
        {
        }

        private static bool IsKnownType(IStateMember stateMember)
        {
            Type type = stateMember.MemberType;

            return type.IsSealed || type.IsValueType;
        }

        public BinaryStateMember(IStateMember stateMember, 
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
                //Type memberType = _stateMember.MemberType;

                //if (IsObject && memberType.IsValueType &&
                //    !memberType.IsPrimitive && memberType != typeof(DateTime))
                //{
                //    return typeof(ValueType);
                //}

                return _stateMember.MemberType;
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

        public Expression GetGetExpression(Expression obj, Expression indices)
        {
            return _stateMember.GetGetExpression(obj, indices);
        }

        public Expression GetSetExpression(Expression obj, Expression indices, Expression value)
        {
            return _stateMember.GetSetExpression(obj, indices, value);
        }

        public IEnumerable<string> Aliases => _stateMember.Aliases;

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