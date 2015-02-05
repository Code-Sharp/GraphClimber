using System;
using System.Collections.Generic;
using System.IO;
using GraphClimber.Bulks;

namespace GraphClimber.Examples
{
    class BinaryWriterProcessor : IPrimitiveProcessor, INullProcessor, IRevisitedProcessor
    {
        public const string NULL_STRING = "Yalla<>Balaghan";
        public const string VISITED_STRING = "Visitied~";
        private readonly BinaryWriter _writer;
        private readonly IDictionary<object, int> _visitedHash = new Dictionary<object, int>();

        public BinaryWriterProcessor(BinaryWriter writer)
        {
            _writer = writer;
        }

        [ProcessorMethod(Precedence = 102)]
        public void Process<T>(IReadOnlyValueDescriptor<T> descriptor)
        {
            T objectToSerialize = descriptor.Get();

            var runtimeType = objectToSerialize.GetType();

            Type memberType = descriptor.StateMember.MemberType;
            if (!memberType.IsSealed || !memberType.IsValueType)
            {
                var assemblyQualifiedName = runtimeType.AssemblyQualifiedName;

                if (string.IsNullOrEmpty(assemblyQualifiedName))
                {
                    throw new NotSupportedException("Serializing types without assembly qualified name is not supported (yet).");
                }

                _writer.Write(assemblyQualifiedName);
            }

            if (runtimeType.IsPrimitive || runtimeType == typeof(string))
            {
                descriptor.Route(new MyCustomStateMember((IReflectionStateMember) descriptor.StateMember, runtimeType), descriptor.Owner);
            }
            else
            {
                descriptor.Climb();
            }
        }


        [ProcessorMethod]
        public void ProcessForReadWrite(IReadWriteValueDescriptor<byte> descriptor)
        {
            _writer.Write(descriptor.Get());
        }

        [ProcessorMethod]
        public void ProcessForReadWrite(IReadWriteValueDescriptor<sbyte> descriptor)
        {
            _writer.Write(descriptor.Get());
        }

        [ProcessorMethod]
        public void ProcessForReadWrite(IReadWriteValueDescriptor<short> descriptor)
        {
            _writer.Write(descriptor.Get());
        }

        [ProcessorMethod]
        public void ProcessForReadWrite(IReadWriteValueDescriptor<ushort> descriptor)
        {
            _writer.Write(descriptor.Get());
        }

        [ProcessorMethod]
        public void ProcessForReadWrite(IReadWriteValueDescriptor<int> descriptor)
        {
            _writer.Write(descriptor.Get());
        }

        [ProcessorMethod]
        public void ProcessForReadWrite(IReadWriteValueDescriptor<uint> descriptor)
        {
            _writer.Write(descriptor.Get());
        }

        [ProcessorMethod]
        public void ProcessForReadWrite(IReadWriteValueDescriptor<long> descriptor)
        {
            _writer.Write(descriptor.Get());
        }

        [ProcessorMethod]
        public void ProcessForReadWrite(IReadWriteValueDescriptor<ulong> descriptor)
        {
            _writer.Write(descriptor.Get());
        }

        [ProcessorMethod]
        public void ProcessForReadWrite(IReadWriteValueDescriptor<char> descriptor)
        {
            _writer.Write(descriptor.Get());
        }

        [ProcessorMethod]
        public void ProcessForReadWrite(IReadWriteValueDescriptor<double> descriptor)
        {
            _writer.Write(descriptor.Get());
        }

        [ProcessorMethod]
        public void ProcessForReadWrite(IReadWriteValueDescriptor<string> descriptor)
        {
            _writer.Write(descriptor.Get());
        }

        [ProcessorMethod]
        public void ProcessForReadWrite(IReadWriteValueDescriptor<DateTime> descriptor)
        {
            _writer.Write(descriptor.Get().Ticks);
        }

        public void ProcessNull<TField>(IWriteOnlyValueDescriptor<TField> descriptor)
        {
            _writer.Write(NULL_STRING);
        }

        public bool Visited(object obj)
        {
            if (_visitedHash.ContainsKey(obj))
            {
                return true;
            }

            _visitedHash[obj] = _visitedHash.Count;
            return false;
        }

        public void ProcessRevisited<TField>(IReadOnlyValueDescriptor<TField> descriptor)
        {
            _writer.Write(VISITED_STRING);
            _writer.Write(_visitedHash[descriptor.Get()]);
        }
    }


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

    internal class PositionRestore : IDisposable
    {
        private readonly Stream _stream;
        private readonly long _position;
        private bool _canceled;

        public PositionRestore(Stream stream)
        {
            _stream = stream;
            _position = stream.Position;
        }

        public void Dispose()
        {
            if (_canceled)
            {
                return;
            }

            _stream.Position = _position;
        }

        public void Cancel()
        {
            _canceled = true;
        }
    }
}
