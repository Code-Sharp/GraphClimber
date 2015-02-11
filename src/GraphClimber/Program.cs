using System;
using System.Collections.Generic;

namespace GraphClimber
{
    class Program
    {
        static void Main(string[] args)
        {
            Person person = new Person()
            {
                Age = 26,
                Name = "Or Bar Yosef",
                Parent = new Person()
                {
                    Name = "Avi",
                    Age = 27,
                    Surprise = new object[]
                    {
                        213,
                        "Shalom",
                        new Person()
                        {
                            Name = "sad"
                        },
                        new int[,]{{1,1,2},{3,5,8}}
                    }
                }
            };

            Extractor<IComparable> myExtractor = new Extractor<IComparable>();

            IEnumerable<IComparable> comparables = myExtractor.Extract(person);

            foreach (IComparable comparable in comparables)
            {
                Console.WriteLine(comparable);
            }

        } 
    }

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
        public virtual void Process<TRuntime>(IReadOnlyValueDescriptor<TRuntime> descriptor)
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

    public class Person
    {
        public string Name { get; set; }
        
        public int Age { get; set; }

        public object Surprise { get; set; }

        public Person Parent { get; set; }
        
        public Person[] Children { get; set; }
    }
}