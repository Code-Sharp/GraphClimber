using System.Collections.Generic;

namespace GraphClimber.Examples
{
    internal class ExtractorProcessor<T> : INullProcessor
    {
        private readonly List<T> _gathered = new List<T>();

        public IEnumerable<T> Result
        {
            get
            {
                return _gathered;
            }
        }

        [ProcessorMethod(Precedence = 1)]
        public virtual void Process<TRuntime>
            (IReadOnlyValueDescriptor<TRuntime> descriptor)
            where TRuntime : T
        {
            T runtime = descriptor.Get();
            _gathered.Add(runtime);
        }

        [ProcessorMethod(Precedence = 2)]
        public void ProcessGeneric<TRuntime>
            (IReadOnlyValueDescriptor<TRuntime> descriptor)
        {
            descriptor.Climb();
        }

        public void ProcessNull<TField>(IWriteOnlyValueDescriptor<TField> descriptor)
        {
            // Don't climb
        }
    }
}