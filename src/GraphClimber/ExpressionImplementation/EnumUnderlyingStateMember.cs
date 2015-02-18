using System;
using System.Linq.Expressions;
using GraphClimber.ExpressionCompiler.Extensions;

namespace GraphClimber
{
    internal class EnumUnderlyingStateMember : IStateMember
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
            get { return _underlying.MemberType.GetEnumUnderlyingType(); }
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

        public bool IsArrayElement
        {
            get { return _underlying.IsArrayElement; }
        }

        public int[] ElementIndex
        {
            get { return _underlying.ElementIndex; }
        }
    }
}