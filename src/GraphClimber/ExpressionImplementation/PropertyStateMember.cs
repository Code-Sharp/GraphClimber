using System;
using System.Linq.Expressions;
using System.Reflection;

namespace GraphClimber
{
    internal class PropertyStateMember : IStateMember
    {
        private readonly PropertyInfo _property;
        private static readonly int[] EmptyIndex = new int[0];

        public PropertyStateMember(PropertyInfo property)
        {
            _property = property;
        }


        public string Name
        {
            get { return _property.Name; }
        }

        public Type OwnerType
        {
            get { return _property.ReflectedType; }
        }

        public virtual Type MemberType
        {
            get { return _property.PropertyType; }
        }

        public bool CanRead
        {
            get { return _property.CanRead; }
        }

        public bool CanWrite
        {
            get { return _property.CanWrite; }
        }

        public Expression GetGetExpression(Expression obj)
        {
            return
                Expression.Property
                    (obj.Convert(_property.DeclaringType),
                        _property);
        }

        public Expression GetSetExpression(Expression obj, Expression value)
        {
            Expression convertInstance;

            if (!_property.DeclaringType.IsValueType)
            {
                convertInstance = 
                    obj.Convert(_property.DeclaringType);
            }
            else
            {
                convertInstance =
                    Expression.Unbox(obj, _property.DeclaringType);
            }

            return
                Expression.Assign(Expression.Property
                    (convertInstance,
                        _property),
                    value.Convert(_property.PropertyType));
        }

        public bool IsArrayElement
        {
            get { return false; }
        }

        public int[] ElementIndex
        {
            get
            {
                return EmptyIndex;
            }
        }

        public MemberInfo UnderlyingMemberInfo
        {
            get
            {
                return _property;
            }
        }

        public object GetValue(object owner)
        {
            return _property.GetValue(owner);
        }

        public void SetValue(object owner, object value)
        {
            _property.SetValue(owner, value);
        }

        protected bool Equals(PropertyStateMember other)
        {
            return Equals(_property, other._property);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PropertyStateMember) obj);
        }

        public override int GetHashCode()
        {
            return (_property != null ? _property.GetHashCode() : 0);
        }
    }
}