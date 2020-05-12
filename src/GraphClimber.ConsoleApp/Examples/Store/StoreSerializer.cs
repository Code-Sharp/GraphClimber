namespace GraphClimber.Examples
{
    public class StoreSerializer
    {
        private readonly IGraphClimber<StoreWriterProcessor> _storeWriterClimber;
        private readonly IGraphClimber<StoreReaderProcessor> _storeReaderClimber;

        public StoreSerializer()
        {
            IStateMemberProvider stateMemberProvider = new PropertyStateMemberProvider();
            _storeWriterClimber = DefaultGraphClimber<StoreWriterProcessor>.Create(stateMemberProvider);
            _storeReaderClimber = DefaultGraphClimber<StoreReaderProcessor>.Create(stateMemberProvider);
        }

        public void Write(object value, IStore store)
        {
            Box<object> box = new Box<object>() {Value = value};
            _storeWriterClimber.Climb(box, new StoreWriterProcessor(store));
        }

        public object Read(IStore store)
        {
            return Read<object>(store);
        }

        public T Read<T>(IStore store)
        {
            Box<T> box = new Box<T>();
            _storeReaderClimber.Climb(box, new StoreReaderProcessor(store));
            return box.Value;
        }
    }
}