using System.Collections.Concurrent;
using System.Collections.Generic;
using GraphClimber.ValueDescriptor;

namespace GraphClimber.Examples
{
    public class StoreWriterProcessor : IRevisitedProcessor, INullProcessor
    {
        private IStore _store;

        private readonly HashSet<object> _visitedObjects = new HashSet<object>();

        public StoreWriterProcessor(IStore store)
        {
            _store = store;
        }

        [ProcessorMethod]
        public void Process<T>(IReadOnlyValueDescriptor<T> descriptor)
        {
            var value = descriptor.Get();
            _store.Set("Type", value.GetType().AssemblyQualifiedName);

            var temp = _store;

            _store = _store.GetInner(descriptor.StateMember.Name);

            descriptor.Climb();

            _store = temp;
        }

        [ProcessorMethod(Precedence = 99)]
        public void ProcessPrimitives<[Primitive]T>(IReadOnlyValueDescriptor<T> descriptor)
        {
            var value = descriptor.Get();
            // _store.Set("Type", value.GetType());
            _store.Set(descriptor.StateMember.Name, value);
        }

        [ProcessorMethod(Precedence = 98)]
        public void ProcessCollection<T>(IReadOnlyValueDescriptor<ICollection<T>> descriptor)
        {
            int i = 0;

            var collection = descriptor.Get();
            _store.Set(descriptor.StateMember.Name + ".Count", collection.Count);

            var temp = _store;
            foreach (var item in collection)
            {
                //_store = temp.GetInner("Item_" + i);
                descriptor.Route(new StaticStateMember(item, "Item_" + i), null, false);

                i++;
            }

            _store = temp;
        }

        public bool Visited(object obj)
        {
            return !_visitedObjects.Add(obj);
        }

        public void ProcessRevisited<TField>(IReadWriteValueDescriptor<TField> descriptor)
        {
            // Does nothing.
        }

        public void ProcessNull<TField>(IWriteOnlyValueDescriptor<TField> descriptor)
        {
            // Does nothing.
        }
    }
}