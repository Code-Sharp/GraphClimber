using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using GraphClimber.ExpressionCompiler.Extensions;

namespace GraphClimber.ExpressionCompiler
{
    public class NonTrivialConstantVisitor : ExpressionVisitor
    {
        private static readonly MethodInfo _getMethodInfo = typeof (NonTrivialConstantVisitor).GetMethod("Get", BindingFlags.Static | BindingFlags.Public);

        private static readonly IList<object> _objects = new List<object>();
        private static readonly object _syncRoot = new object();

        public static readonly ExpressionVisitor Empty = new NonTrivialConstantVisitor();

        private NonTrivialConstantVisitor()
        {
            
        }

        public static object Get(int number)
        {
            return _objects[number];
        }

        public static int Add<T>(T value)
        {
            lock (_syncRoot)
            {
                int objectNumber = _objects.Count;

                _objects.Add(value);

                return objectNumber;
            }
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (node.Type.IsPrimitive || node.Type == typeof(string))
            {
                return base.VisitConstant(node);
            }

            if (node.Type.FullName == "System.RuntimeType")
            {
                return Expression.Constant(node.Value, typeof (Type));
            }

            int objectNumber = Add(node.Value);

            return Expression.Call(_getMethodInfo, objectNumber.Constant()).Convert(node.Type);
        }
    }
}
