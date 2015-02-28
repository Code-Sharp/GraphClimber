using System;
using System.Linq.Expressions;
using System.Reflection;
using GraphClimber.ExpressionCompiler.Extensions;

namespace GraphClimber
{
    internal class EnumUnderlyingStateMember<TEnum> : IStateMember, IReflectionStateMember
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

        public Expression GetGetExpression(Expression obj)
        {
            return _underlying.GetGetExpression(obj).Convert(MemberType);
        }

        public Expression GetSetExpression(Expression obj, Expression value)
        {
            return _underlying.GetSetExpression(obj, value.Convert(_underlying.MemberType));
        }

        public Action<object, T> BuildSetterForBox<T>()
        {
            // TODO: make this "BuildSetterForBox" thingy more composite,
            // TODO: so we won't call for 3 methods for a setter of a boxed enum.
            Action<object, TEnum> underlyingSet = 
                _underlying.BuildSetterForBox<TEnum>();

            Func<T, TEnum> toEnum = EnumConvert<TEnum, T>.ToEnum;

            Action<object, T> result = (instance, value) =>
                underlyingSet(instance, toEnum(value));

            return result;
        }

        public bool IsArrayElement
        {
            get { return _underlying.IsArrayElement; }
        }

        public int[] ElementIndex
        {
            get { return _underlying.ElementIndex; }
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

        public MemberInfo UnderlyingMemberInfo
        {
            get
            {
                var underlying = _underlying as IReflectionStateMember;

                if (underlying != null)
                {
                    return underlying.UnderlyingMemberInfo;
                }

                throw new NotImplementedException();
            }
        }

        public object GetValue(object owner)
        {
            var underlying = _underlying as IReflectionStateMember;

            if (underlying != null)
            {
                return underlying.GetValue(owner);
            }

            throw new NotImplementedException();
        }

        public void SetValue(object owner, object value)
        {
            var underlying = _underlying as IReflectionStateMember;

            if (underlying != null)
            {
                underlying.SetValue(owner, value);
            }
            else
            {
                throw new NotImplementedException();                
            }
        }
    }
}