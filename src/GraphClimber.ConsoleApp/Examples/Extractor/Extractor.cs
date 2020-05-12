using System;

namespace GraphClimber.Examples
{
    public class Extractor<T>
    {
        private readonly IGraphClimber<ExtractorProcessor<T>> _climber;

        public Extractor()
        {
            _climber =
                DefaultGraphClimber<ExtractorProcessor<T>>
                    .Create(new PropertyStateMemberProvider());
        }

        public void Extract(object value, Action<T> callback)
        {
            ExtractorProcessor<T> processor = new ExtractorProcessor<T>(callback);

            _climber.Climb(new Box<object>() {Value = value}, processor);
        }
    }
}