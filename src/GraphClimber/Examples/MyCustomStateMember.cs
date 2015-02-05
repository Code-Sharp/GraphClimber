using System;
using System.Linq.Expressions;

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

        public Expression GetGetExpression(Expression obj)
        {
            return _underlying.GetGetExpression(obj);
        }

        public Expression GetSetExpression(Expression obj, Expression value)
        {
            return _underlying.GetSetExpression(obj, value);
        }

        public object GetValue(object owner)
        {
            return _underlying.GetValue(owner);
        }

        public void SetValue(object owner, object value)
        {
            _underlying.SetValue(owner, value);
        }
    }
}