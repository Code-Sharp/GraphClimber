using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace GraphClimber.ValueDescriptor
{
    public class StaticStateMember : IStateMember
    {
        private readonly object _value;
        private readonly string _name;

        public StaticStateMember(object value, string name = "Value")
        {
            _value = value;
            _name = name;
        }

        public string Name
        {
            get { return _name; }
        }

        public Type OwnerType
        {
            get { throw new NotImplementedException(); }
        }

        public Type MemberType
        {
            get { return typeof(object); }
        }

        public bool CanRead
        {
            get { return true; }
        }

        public bool CanWrite
        {
            get { return false; }
        }

        public Expression GetGetExpression(Expression obj, Expression indices)
        {
            return _value.Constant();
        }

        public Expression GetSetExpression(Expression obj, Expression indices, Expression value)
        {
            return Expression.Empty();
        }

        public IEnumerable<string> Aliases
        {
            get
            {
                yield return Name;
            }
        }
    }

    public class StrongBoxStateMember<T> : IStateMember
    {
        private readonly StrongBox<T> _strongBox;
        private readonly string _name;

        public StrongBoxStateMember(StrongBox<T> strongBox, string name = "Value")
        {
            _strongBox = strongBox;
            _name = name;
        }

        public string Name
        {
            get { return _name; }
        }

        public Type OwnerType
        {
            get { throw new NotImplementedException(); }
        }

        public Type MemberType
        {
            get { return typeof (T); }
        }

        public bool CanRead
        {
            get { return true; }
        }

        public bool CanWrite
        {
            get { return true; }
        }

        public Expression GetGetExpression(Expression obj, Expression indices)
        {
            throw new NotImplementedException();
        }

        public Expression GetSetExpression(Expression obj, Expression indices, Expression value)
        {
            throw new NotImplementedException();
        }

        public bool IsArrayElement
        {
            get { return false; }
        }

        public int[] ElementIndex
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerable<string> Aliases
        {
            get
            {
                yield return Name;
            }
        }
        public MemberInfo UnderlyingMemberInfo
        {
            get { throw new NotImplementedException(); }
        }

        public object GetValue(object owner)
        {
            return _strongBox.Value;
        }

        public void SetValue(object owner, object value)
        {
            _strongBox.Value = (T) value;
        }
    }
}
