using System;
using System.Linq.Expressions;
using System.Reflection;

namespace GraphClimber
{
    internal class ArrayStateMember : IStateMember
    {
        private readonly Type _arrayType;
        private readonly Type _arrayElementType;
        private readonly int[] _indices;

        public ArrayStateMember(Type arrayType, Type arrayElementType, int[] indices)
        {
            _arrayType = arrayType;
            _arrayElementType = arrayElementType;
            _indices = indices;
        }

        public string Name
        {
            get
            {
                return string.Format("[{0}]",
                                     string.Join(", ", _indices));
            }
        }

        public Type OwnerType
        {
            get
            {
                return _arrayType;
            }
        }

        public Type MemberType
        {
            get
            {
                return _arrayElementType;
            }
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
            get
            {
                return true;
            }
        }

        public int[] ElementIndex
        {
            get
            {
                return _indices;
            }
        }
    }
}