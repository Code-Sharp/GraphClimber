using System;

namespace GraphClimber
{
    class Program
    {
        static void Main(string[] args)
        {
        }
    }


    public interface IGraphClimber<TProcessor>
    {

        void Climb(object o, TProcessor processor);

    }

    public class MyInheritedProcessor : IWriteProcessor<IAsyncResult>, IProcessor<int[]>, IRevisitedFilter
    {
        public void ProcessForWrite(IWriteValueDescriptor<IAsyncResult> descriptor)
        {
            // Sets all fields that are assignable from IAsyncResult to null.
            // (i.e: all fields that have static type object or IAsyncResult)
            descriptor.Set((IAsyncResult)null);
        }

        public void ProcessForReadWrite(IReadWriteValueDescriptor<int[]> descriptor)
        {
            int[] value = descriptor.Get();
            value[0] = 4;
            descriptor.Set(value);
        }

        public bool Visited(object obj)
        {
            return false;
        }
    }

    public class MyInheritedProcessor2 : IReadWriteProcessor<IAsyncResult>
    {
        public void ProcessForReadWrite(IReadWriteValueDescriptor<IAsyncResult> descriptor)
        {
            // Sets all field that are assignable from IAsyncResult and are
            // actually currently from type IAsyncResult, to null.
            descriptor.Set((IAsyncResult)null);
        }
    }
}
