using System;
using System.Linq.Expressions;

namespace GraphClimber.ValueDescriptor
{
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

        public Expression GetGetExpression(Expression obj)
        {
            return Expression.Invoke(Expression.Constant(_getValue));
        }

        public Expression GetSetExpression(Expression obj, Expression value)
        {
            return Expression.Invoke(Expression.Constant(_setValue), value);
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

        public Expression GetGetExpression(Expression obj)
        {
            return Expression.Invoke(Expression.Constant(_getValue));
        }

        public Expression GetSetExpression(Expression obj, Expression value)
        {
            return Expression.Invoke(Expression.Constant(_setValue), value);
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
