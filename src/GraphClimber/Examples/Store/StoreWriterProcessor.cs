namespace GraphClimber.Examples
{
    public class StoreWriterProcessor 
    {
        private IStore _store;

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

    }
}