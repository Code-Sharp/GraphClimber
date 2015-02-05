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
}
