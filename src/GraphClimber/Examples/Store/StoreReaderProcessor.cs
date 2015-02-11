using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using GraphClimber.ValueDescriptor;

namespace GraphClimber.Examples
{
    public class StoreReaderProcessor
    {
        private IStore _store;

        public StoreReaderProcessor(IStore store)
        {
            _store = store;
        }

        [ProcessorMethod(Precedence = 102)]
        public void Process<T>(IWriteOnlyValueDescriptor<T> descriptor)
        {
            string type;
            if (_store.TryGet("Type", out type))
            {

                var value = (T)Activator.CreateInstance(Type.GetType(type));

                descriptor.Set(value);

                var temp = _store;
                _store = _store.GetInner(descriptor.StateMember.Name);

                descriptor.Climb();

                _store = temp;
            }
        }

        [ProcessorMethod(Precedence = 99)]
        public void ProcessPrimitives<[Primitive]T>(IWriteOnlyExactValueDescriptor<T> descriptor)
        {
            T value;
            if (_store.TryGet<T>(descriptor.StateMember.Name, out value))
            {
                descriptor.Set(value);
            }
            
        }

        [ProcessorMethod]
        public void Process(IWriteOnlyValueDescriptor<object> descriptor)
        {
            string type;

            if (_store.TryGet("Type", out type))
            { 
                descriptor.Route(new MyCustomStateMember((IReflectionStateMember)descriptor.StateMember, Type.GetType(type)), descriptor.Owner, true);
            }
        }

        [ProcessorMethod(Precedence = 98)]
        public void ProcessCollection<T>(IReadOnlyValueDescriptor<ICollection<T>> descriptor)
        {
            int count;
            var collection = descriptor.Get();

            if (!_store.TryGet(descriptor.StateMember.Name + ".Count", out count))
            {
                return;
            }


            var temp = _store;

            for (int i = 0; i < count; i++)
            {
                var strongBox = new StrongBox<T>(default(T));

                descriptor.Route(new StrongBoxStateMember<T>(strongBox, "Item_" + i),
                    typeof(T), strongBox, false);

                collection.Add(strongBox.Value);
            }

            _store = temp;
        }
    }
}