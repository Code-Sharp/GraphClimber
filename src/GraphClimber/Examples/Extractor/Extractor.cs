using System;

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

        public void Extract(object value, Action<T> callback)
        {
            ExtractorProcessor<T> processor = new ExtractorProcessor<T>(callback);

            _climber.Route(value, processor, false);
        }
    }
}