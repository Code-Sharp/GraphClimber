using System;

namespace GraphClimber
{

    public interface IGraphClimber<TProcessor>
    {

        void Climb(object o, TProcessor processor);

    }

    public class MyInheritedProcessor : IProcessor<int[]>, IRevisitedFilter
//	, IInheritedProcessor<IAsyncResult>
    {

		// This thing doesn't compile on mono for some reason...
//        public void Process<TReal>(TReal value, IValueDescriptor<IAsyncResult> descriptor) where TReal : IAsyncResult
//        {
//            descriptor.Set((IAsyncResult)null);
//        }

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
