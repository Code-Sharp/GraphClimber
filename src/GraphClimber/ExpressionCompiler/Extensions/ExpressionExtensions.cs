using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace GraphClimber.ExpressionCompiler.Extensions
{
    public static class ExpressionExtensions
    {

        public static Expression Constant<T>(this T value)
        {
            return Expression.Constant(value, typeof (T));
        }



    }
}
