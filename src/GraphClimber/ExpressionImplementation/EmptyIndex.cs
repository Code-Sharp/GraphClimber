using System.Linq.Expressions;

namespace GraphClimber
{
    internal class EmptyIndex
    {
        public static readonly int[] Value = new int[0];

        public static readonly Expression Constant = Value.Constant();
    }
}