using System;
using System.Collections.Generic;

namespace GraphClimber.Examples
{
    internal class ExtractorProcessor<T> : INullProcessor
    {
        private readonly Action<T> _callback;

        public ExtractorProcessor(Action<T> callback)
        {
            _callback = callback;
        }

        [ProcessorMethod(Precedence = 1)]
        public virtual void Process<TRuntime>
            (IReadOnlyValueDescriptor<TRuntime> descriptor)
            where TRuntime : T
        {
            T runtime = descriptor.Get();
            _callback(runtime);
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