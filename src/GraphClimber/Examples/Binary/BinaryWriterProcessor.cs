using System;
using System.Collections.Generic;
using System.IO;
using GraphClimber.Bulks;

namespace GraphClimber.Examples
{
    public static class ReadWriteHeader
    {
        public const byte Null = 1;
        public const byte Revisited = 2;
        public const byte KnownType = 3;
        public const byte UnknownType = 4;
    }

    class BinaryWriterProcessor : IReadOnlyPrimitiveProcessor, INullProcessor, IRevisitedProcessor
    {
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

            BinaryStateMember member = descriptor.StateMember as BinaryStateMember;
            
            WriteAssemblyQualifiedNameIfNeeded(member, runtimeType);

            descriptor.Climb();
        }

        [ProcessorMethod(Precedence = 97)]
        // Processes when the field type is exactly object.
        public void ProcessObject(IReadWriteValueDescriptor<object> descriptor)
        {
            object objectToSerialize = descriptor.Get();
            
            BinaryStateMember member = descriptor.StateMember as BinaryStateMember;
            
            Type runtimeType = objectToSerialize.GetType();
            
            WriteAssemblyQualifiedNameIfNeeded(member, runtimeType);

            descriptor.Route(
                new BinaryStateMember(
                    new MyCustomStateMember((IReflectionStateMember) descriptor.StateMember, runtimeType),
                    true,
                    true),
                descriptor.Owner);
        }

        private void WriteAssemblyQualifiedNameIfNeeded(BinaryStateMember member, Type runtimeType)
        {
            if (member != null && member.HeaderHandled)
            {
                return;
            }

            // If member == null is a patch because the way we implemented route in 
            // SlowGraphClimber.
            if ((member != null) &&
                ((member.KnownType) ||
                 (member.MemberType == runtimeType)))
            {
                _writer.Write(ReadWriteHeader.KnownType);
            }
            else
            {
                string assemblyQualifiedName = runtimeType.AssemblyQualifiedName;

                if (string.IsNullOrEmpty(assemblyQualifiedName))
                {
                    throw new NotSupportedException(
                        "Serializing types without assembly qualified name is not supported (yet).");
                }

                _writer.Write(ReadWriteHeader.UnknownType);
                _writer.Write(assemblyQualifiedName);
            }
        }

        [ProcessorMethod]
        public void ProcessForReadOnly(IReadOnlyValueDescriptor<byte> descriptor)
        {
            _writer.Write(descriptor.Get());
        }

        [ProcessorMethod]
        public void ProcessForReadOnly(IReadOnlyValueDescriptor<sbyte> descriptor)
        {
            _writer.Write(descriptor.Get());
        }

        [ProcessorMethod]
        public void ProcessForReadOnly(IReadOnlyValueDescriptor<short> descriptor)
        {
            _writer.Write(descriptor.Get());
        }

        [ProcessorMethod]
        public void ProcessForReadOnly(IReadOnlyValueDescriptor<ushort> descriptor)
        {
            _writer.Write(descriptor.Get());
        }

        [ProcessorMethod]
        public void ProcessForReadOnly(IReadOnlyValueDescriptor<int> descriptor)
        {
            _writer.Write(descriptor.Get());
        }

        [ProcessorMethod]
        public void ProcessForReadOnly(IReadOnlyValueDescriptor<uint> descriptor)
        {
            _writer.Write(descriptor.Get());
        }

        [ProcessorMethod]
        public void ProcessForReadOnly(IReadOnlyValueDescriptor<long> descriptor)
        {
            _writer.Write(descriptor.Get());
        }

        [ProcessorMethod]
        public void ProcessForReadOnly(IReadOnlyValueDescriptor<ulong> descriptor)
        {
            _writer.Write(descriptor.Get());
        }

        [ProcessorMethod]
        public void ProcessForReadOnly(IReadOnlyValueDescriptor<char> descriptor)
        {
            _writer.Write(descriptor.Get());
        }

        [ProcessorMethod]
        public void ProcessForReadOnly(IReadOnlyValueDescriptor<double> descriptor)
        {
            _writer.Write(descriptor.Get());
        }

        [ProcessorMethod]
        public void ProcessForReadOnly(IReadOnlyValueDescriptor<string> descriptor)
        {
            WriteAssemblyQualifiedNameIfNeeded(descriptor.StateMember as BinaryStateMember,
                typeof (string));

            _writer.Write(descriptor.Get());
        }

        [ProcessorMethod]
        public void ProcessForReadOnly(IReadOnlyValueDescriptor<DateTime> descriptor)
        {
            _writer.Write(descriptor.Get().Ticks);
        }

        public void ProcessNull<TField>(IWriteOnlyValueDescriptor<TField> descriptor)
        {
            _writer.Write((byte) ReadWriteHeader.Null);
        }

        public bool Visited(object obj)
        {
            if (_visitedHash.ContainsKey(obj))
            {
                return true;
            }

            Type type = obj.GetType();

            if (!type.IsPrimitive && type != typeof(DateTime))
            {
                _visitedHash[obj] = _visitedHash.Count;                
            }

            return false;
        }

        public void ProcessRevisited<TField>(IReadWriteValueDescriptor<TField> descriptor)
        {
            _writer.Write((byte) ReadWriteHeader.Revisited);
            _writer.Write(_visitedHash[descriptor.Get()]);
        }
    }
}
