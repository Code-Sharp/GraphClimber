using System;
using System.Collections.Generic;
using System.IO;
using GraphClimber.Bulks;

namespace GraphClimber.Examples
{
    class BinaryReaderProcessor : IWriteOnlyExactPrimitiveProcessor
    {
        private readonly IList<object> _objects = new List<object>();
        private readonly BinaryReader _reader;

        public BinaryReaderProcessor(BinaryReader reader)
        {
            _reader = reader;
        }

        [ProcessorMethod(Precedence = 102)]
        public void Process<T>(IWriteOnlyValueDescriptor<T> descriptor)
        {
            Type type = descriptor.StateMember.MemberType;

            BinaryStateMember member = descriptor.StateMember as BinaryStateMember;
   
            if (!member.KnownType)
            {
                string assemblyQualifiedName = _reader.ReadString();
                type = Type.GetType(assemblyQualifiedName);

                if (assemblyQualifiedName == BinaryWriterProcessor.NULL_STRING)
                {
                    return;
                }

                if (assemblyQualifiedName == BinaryWriterProcessor.VISITED_STRING)
                {
                    HandleVisited(descriptor);
                    return;
                }
            }
            else
            {
                using (var restore = PositionRestore(_reader))
                {
                    var stringRead = _reader.ReadString();
                    var isNull = stringRead == BinaryWriterProcessor.NULL_STRING;
                    if (isNull)
                    {
                        restore.Cancel();
                        return;
                    }

                    var isRevisited = stringRead == BinaryWriterProcessor.VISITED_STRING;

                    if (isRevisited)
                    {
                        HandleVisited(descriptor);
                        restore.Cancel();
                        return;
                    }
                }
            }

            if (type.IsValueType)
            {
                descriptor.Route(new MyCustomStateMember((IReflectionStateMember)descriptor.StateMember, type), descriptor.Owner);
            }
            else
            {
                var instance = (T)Activator.CreateInstance(type);
                _objects.Add(instance);
                descriptor.Set(instance); 
                descriptor.Climb();
            }
        }

        private void HandleVisited<T>(IWriteOnlyValueDescriptor<T> descriptor)
        {
            descriptor.Set((T) _objects[_reader.ReadInt32() - 1]);
        }

        private PositionRestore PositionRestore(BinaryReader reader)
        {
            return new PositionRestore(reader.BaseStream);
        }

        [ProcessorMethod]
        public void ProcessForWriteOnly(IWriteOnlyExactValueDescriptor<byte> descriptor)
        {
            descriptor.Set(_reader.ReadByte());
        }

        [ProcessorMethod]
        public void ProcessForWriteOnly(IWriteOnlyExactValueDescriptor<sbyte> descriptor)
        {
            descriptor.Set(_reader.ReadSByte());
        }

        [ProcessorMethod]
        public void ProcessForWriteOnly(IWriteOnlyExactValueDescriptor<short> descriptor)
        {
            descriptor.Set(_reader.ReadInt16());
        }

        [ProcessorMethod]
        public void ProcessForWriteOnly(IWriteOnlyExactValueDescriptor<ushort> descriptor)
        {
            descriptor.Set(_reader.ReadUInt16());
        }

        [ProcessorMethod]
        public void ProcessForWriteOnly(IWriteOnlyExactValueDescriptor<int> descriptor)
        {
            descriptor.Set(_reader.ReadInt32());
        }

        [ProcessorMethod]
        public void ProcessForWriteOnly(IWriteOnlyExactValueDescriptor<uint> descriptor)
        {
            descriptor.Set(_reader.ReadUInt32());
        }

        [ProcessorMethod]
        public void ProcessForWriteOnly(IWriteOnlyExactValueDescriptor<long> descriptor)
        {
            descriptor.Set(_reader.ReadInt64());
        }

        [ProcessorMethod]
        public void ProcessForWriteOnly(IWriteOnlyExactValueDescriptor<ulong> descriptor)
        {
            descriptor.Set(_reader.ReadUInt64());
        }

        [ProcessorMethod]
        public void ProcessForWriteOnly(IWriteOnlyExactValueDescriptor<char> descriptor)
        {
            descriptor.Set(_reader.ReadChar());
        }

        [ProcessorMethod]
        public void ProcessForWriteOnly(IWriteOnlyExactValueDescriptor<double> descriptor)
        {
            descriptor.Set(_reader.ReadDouble());
        }

        [ProcessorMethod]
        public void ProcessForWriteOnly(IWriteOnlyExactValueDescriptor<string> descriptor)
        {
            descriptor.Set(_reader.ReadString());
        }

        [ProcessorMethod]
        public void ProcessForWriteOnly(IWriteOnlyExactValueDescriptor<DateTime> descriptor)
        {
            var ticks = _reader.ReadInt64();
            descriptor.Set(DateTime.FromBinary(ticks));
        }

    }
}