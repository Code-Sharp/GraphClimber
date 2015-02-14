using System;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using GraphClimber.ExpressionCompiler.Extensions;

namespace GraphClimber.ValueDescriptor
{
    public class StaticStateMember : IReflectionStateMember
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

        public Expression GetGetExpression(Expression obj)
        {
            return _value.Constant();
        }

        public Expression GetSetExpression(Expression obj, Expression value)
        {
            return Expression.Empty();
        }

        public bool IsArrayElement
        {
            get { return false; }
        }

        public int[] ElementIndex
        {
            get { throw new NotImplementedException(); }
        }

        public object GetValue(object owner)
        {
            return _value;
        }

        public void SetValue(object owner, object value)
        {
            // Does nothing!
        }
    }

    public class StrongBoxStateMember<T> : IReflectionStateMember
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

        public Expression GetGetExpression(Expression obj)
        {
            throw new NotImplementedException();
        }

        public Expression GetSetExpression(Expression obj, Expression value)
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

        public object GetValue(object owner)
        {
            return _strongBox.Value;
        }

        public void SetValue(object owner, object value)
        {
            _strongBox.Value = (T) value;
        }
    }

    public class DelegateStateMember : IReflectionStateMember
    {
        private readonly Type _memberType;
        private readonly Func<object> _getValue;
        private readonly Action<object> _setValue;

        public DelegateStateMember(Type memberType, Func<object> getValue, Action<object> setValue)
        {
            _memberType = memberType;
            _getValue = getValue;
            _setValue = setValue;
        }


        public string Name
        {
            get { return "Value"; }
        }

        public Type OwnerType
        {
            get { throw new NotSupportedException(); }
        }

        public Type MemberType
        {
            get { return _memberType; }
        }

        public bool CanRead
        {
            get { return true; }
        }

        public bool CanWrite
        {
            get { return true; }
        }

        public Expression GetGetExpression(Expression obj)
        {
            return Expression.Invoke(_getValue.Constant());
        }

        public Expression GetSetExpression(Expression obj, Expression value)
        {
            return Expression.Invoke(_setValue.Constant(), value);
        }

        public bool IsArrayElement
        {
            get { return false; }
        }

        public int[] ElementIndex
        {
            get { throw new NotSupportedException(); }
        }

        public object GetValue(object owner)
        {
            return _getValue();
        }

        public void SetValue(object owner, object value)
        {
            _setValue(value);
        }

        public static IReflectionStateMember Create(Type memberType, Func<object> getValue, Action<object> setValue)
        {
            return new DelegateStateMember(memberType, getValue, setValue);
        }

        public static IReflectionStateMember Create<T>(Func<T> getValue, Action<T> setValue)
        {
            return new SafeDelegateStateMember<T>(getValue, setValue);
        }
    }

    public class SafeDelegateStateMember<T> : IReflectionStateMember
    {
        private readonly Func<T> _getValue;
        private readonly Action<T> _setValue;

        public SafeDelegateStateMember(Func<T> getValue, Action<T> setValue)
        {
            _getValue = getValue;
            _setValue = setValue;
        }

        public string Name
        {
            get { return "Value"; }
        }

        public Type OwnerType
        {
            get { throw new NotSupportedException(); }
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

        public Expression GetGetExpression(Expression obj)
        {
            return Expression.Invoke(_getValue.Constant());
        }

        public Expression GetSetExpression(Expression obj, Expression value)
        {
            return Expression.Invoke(_setValue.Constant(), value);
        }

        public bool IsArrayElement
        {
            get { return false; }
        }

        public int[] ElementIndex
        {
            get { throw new NotSupportedException(); }
        }

        public object GetValue(object owner)
        {
            return _getValue();
        }

        public void SetValue(object owner, object value)
        {
            _setValue((T) value);
        }
    }
}
