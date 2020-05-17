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
        private readonly int _arrayRank;

        public ArrayStateMember(Type arrayType, Type arrayElementType, int arrayRank)
        {
            _arrayType = arrayType;
            _arrayElementType = arrayElementType;
            _arrayRank = arrayRank;
        }

        public string Name
        {
            get
            {
                return _arrayType.FullName;
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

        public Expression GetGetExpression(Expression obj, Expression indices)
        {
            Expression getExpression = ArrayAccess(obj, indices);
            return getExpression;
        }

        private Expression ArrayAccess(Expression obj, Expression indices)
        {
            IEnumerable<BinaryExpression> indexAccess =
                Enumerable.Range(0, _arrayRank)
                          .Select(x => Expression.ArrayIndex(indices,
                                                             Expression.Constant(x)));

            return Expression.ArrayAccess(obj.Convert(this._arrayType), indexAccess);
        }

        public Expression GetSetExpression(Expression obj, Expression indices, Expression value)
        {
            Expression arrayAccess = ArrayAccess(obj, indices);
            return Expression.Assign(arrayAccess, value);
        }

        public IEnumerable<string> Aliases
        {
            get
            {
                yield break;
            }
        }
    }
}