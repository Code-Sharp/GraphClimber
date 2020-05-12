using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using GraphClimber.Examples;
using GraphClimber.Examples.Binary;

namespace GraphClimber.ConsoleApp.Examples.Binary
{
    public class BinaryFormatter
    {
        private readonly IGraphClimber<BinaryReaderProcessor> _binaryReader;
        private readonly IGraphClimber<BinaryWriterProcessor> _binaryWriter;

        public BinaryFormatter()
        {
            IStateMemberProvider provider = new BinaryStateMemberProvider(new PropertyStateMemberProvider());
            _binaryReader = DefaultGraphClimber<BinaryReaderProcessor>.Create(provider);
            _binaryWriter = DefaultGraphClimber<BinaryWriterProcessor>.Create(provider);
        }

        public object Deserialize(IReader reader)
        {
            return Deserialize<object>(reader);
        }

        public T Deserialize<T>(IReader reader)
        {
            BinaryReaderProcessor processor = new BinaryReaderProcessor(reader);
            StrongBox<T> box = new StrongBox<T>();
            _binaryReader.Climb(box, processor);
            return box.Value;
        }

        public void Serialize(IWriter writer, object graph)
        {
            Serialize<object>(writer, graph);
        }

        public void Serialize<T>(IWriter writer, T graph)
        {
            BinaryWriterProcessor processor = new BinaryWriterProcessor(writer);
            StrongBox<T> box = new StrongBox<T>(){Value = graph};
            _binaryWriter.Climb(box, processor);
        }
    }
}