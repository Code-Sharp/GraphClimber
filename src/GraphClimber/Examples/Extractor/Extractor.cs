using System.Collections.Generic;

namespace GraphClimber.Examples
{
    public class Extractor<T>
    {
        private readonly SlowGraphClimber<ExtractorProcessor<T>> _climber;

        public Extractor()
        {
            _climber =
                new SlowGraphClimber<ExtractorProcessor<T>>(new ReflectionPropertyStateMemberProvider());
        }

        public IEnumerable<T> Extract(object value)
        {
            ExtractorProcessor<T> processor = new ExtractorProcessor<T>();

            _climber.Route(value, processor, false);

            return processor.Result;
        }
    }
}