using System;
using System.Collections.Generic;
using System.Reflection;

namespace GraphClimber
{
    class Program
    {
        static void Main(string[] args)
        {
            MethodInfo hey;
            new GenericArgumentBinder().TryBind(typeof (IMyGenerics).GetMethods()[0], new []{typeof(List<List<int>>), typeof(List<int>)}, out hey);
        }
    }


    public interface IGraphClimber<TProcessor>
    {

        void Climb(object o, TProcessor processor);

    }

    public class MyInheritedProcessor : IInheritedProcessor<IAsyncResult>, IProcessor<int[]>, IRevisitedFilter
    {

        public void Process<TReal>(TReal value, IValueDescriptor<IAsyncResult> descriptor) where TReal : IAsyncResult
        {
            descriptor.Set((IAsyncResult)null);
        }

        public void Process(int[] value, IValueDescriptor<int[]> descriptor)
        {
            value[0] = 4;
            descriptor.Set(value);
        }

        public bool Visited(object obj)
        {
            return false;
        }
    }
}
