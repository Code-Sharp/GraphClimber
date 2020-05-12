using System;
using System.Collections.Generic;
using System.Linq;
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
            Expression getExpression = ArrayAccess(obj);
            return getExpression;
        }

        private Expression ArrayAccess(Expression obj)
        {
            ConstantExpression array = Expression.Constant(_indices, typeof(int[]));

            IEnumerable<BinaryExpression> indices =
                Enumerable.Range(0, _indices.Length)
                          .Select(x => Expression.ArrayIndex(array,
                                                             Expression.Constant(x)));
            return Expression.ArrayAccess(obj.Convert(this._arrayType), indices);
        }

        public Expression GetSetExpression(Expression obj, Expression value)
        {
            Expression arrayAccess = ArrayAccess(obj);
            return Expression.Assign(arrayAccess, value);
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