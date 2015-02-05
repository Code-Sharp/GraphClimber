using System;
using System.Collections.Generic;
using System.IO;
using GraphClimber.Bulks;

namespace GraphClimber.Examples
{
    class BinaryReaderProcessor : IPrimitiveProcessor
    {
        private readonly IList<object> _objects = new List<object>();
        private readonly BinaryReader _reader;

        public BinaryReaderProcessor(BinaryReader reader)
        {
            _reader = reader;
        }

        //[ProcessorMethod(Precedence = 101)]
        //public void ProcessExactAsField<[Super(typeof(object))] T>(IReadWriteValueDescriptor<T> descriptor)
        //{
        //    descriptor.Climb();
        //}

        [ProcessorMethod(Precedence = 102)]
        public void Process<T>(IWriteOnlyValueDescriptor<T> descriptor)
        {
            Type memberType = descriptor.StateMember.MemberType;
            Type type = descriptor.StateMember.MemberType;
            
            if (!memberType.IsSealed || !memberType.IsValueType)
            {
                var assemblyQualifiedName = _reader.ReadString();
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
        public void ProcessForReadWrite(IReadWriteValueDescriptor<byte> descriptor)
        {
            descriptor.Set(_reader.ReadByte());
        }

        [ProcessorMethod]
        public void ProcessForReadWrite(IReadWriteValueDescriptor<sbyte> descriptor)
        {
            descriptor.Set(_reader.ReadSByte());
        }

        [ProcessorMethod]
        public void ProcessForReadWrite(IReadWriteValueDescriptor<short> descriptor)
        {
            descriptor.Set(_reader.ReadInt16());
        }

        [ProcessorMethod]
        public void ProcessForReadWrite(IReadWriteValueDescriptor<ushort> descriptor)
        {
            descriptor.Set(_reader.ReadUInt16());
        }

        [ProcessorMethod]
        public void ProcessForReadWrite(IReadWriteValueDescriptor<int> descriptor)
        {
            descriptor.Set(_reader.ReadInt32());
        }

        [ProcessorMethod]
        public void ProcessForReadWrite(IReadWriteValueDescriptor<uint> descriptor)
        {
            descriptor.Set(_reader.ReadUInt32());
        }

        [ProcessorMethod]
        public void ProcessForReadWrite(IReadWriteValueDescriptor<long> descriptor)
        {
            descriptor.Set(_reader.ReadInt64());
        }

        [ProcessorMethod]
        public void ProcessForReadWrite(IReadWriteValueDescriptor<ulong> descriptor)
        {
            descriptor.Set(_reader.ReadUInt64());
        }

        [ProcessorMethod]
        public void ProcessForReadWrite(IReadWriteValueDescriptor<char> descriptor)
        {
            descriptor.Set(_reader.ReadChar());
        }

        [ProcessorMethod]
        public void ProcessForReadWrite(IReadWriteValueDescriptor<double> descriptor)
        {
            descriptor.Set(_reader.ReadDouble());
        }

        [ProcessorMethod]
        public void ProcessForReadWrite(IReadWriteValueDescriptor<string> descriptor)
        {
            descriptor.Set(_reader.ReadString());
        }

        [ProcessorMethod]
        public void ProcessForReadWrite(IReadWriteValueDescriptor<DateTime> descriptor)
        {
            var ticks = _reader.ReadInt64();
            descriptor.Set(DateTime.FromBinary(ticks));
        }

    }
}