using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace GraphClimber
{
    internal class PropertyStateMember : IStateMember
    {
        private readonly PropertyInfo _property;

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

        public Expression GetGetExpression(Expression obj, Expression indices)
        {
            return
                Expression.Property
                    (obj.Convert(_property.DeclaringType),
                     _property);
        }

        public Expression GetSetExpression(Expression obj, Expression indices, Expression value)
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


        public IEnumerable<string> Aliases
        {
            get
            {
                yield return Name;
            }
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