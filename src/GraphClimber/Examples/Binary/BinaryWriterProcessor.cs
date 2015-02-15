using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using GraphClimber.Bulks;
using GraphClimber.ValueDescriptor;

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

        [ProcessorMethod(OnlyOnRoute = true, Precedence = 1)]
        public void ProcessObjectOnly(IReadOnlyExactValueDescriptor<object> descriptor)
        {
            // Does nothing.
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
                descriptor.Owner,
                true);
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

        private void WriteAssemblyQualifiedNameIfNeeded<T>(IReadOnlyValueDescriptor<T> descriptor)
        {
            WriteAssemblyQualifiedNameIfNeeded
                (descriptor.StateMember as BinaryStateMember,
                typeof(T));
        }

        private void WriteIntArray(int[] indices)
        {
            _writer.Write(indices.Length);

            foreach (int index in indices)
            {
                _writer.Write(index);
            }
        }

        [ProcessorMethod]
        public void ProcessArray<[IsArray]TArray>(IReadOnlyValueDescriptor<TArray> descriptor)
        {
            WriteAssemblyQualifiedNameIfNeeded(descriptor);

            Array array = descriptor.Get() as Array;

            int rank = array.Rank;

            int[] lowerIndicies =
                Enumerable.Range(0, rank)
                    .Select(array.GetLowerBound)
                    .ToArray();

            int[] lengths =
                Enumerable.Range(0, rank)
                    .Select(array.GetLength)
                    .ToArray();

            WriteIntArray(lowerIndicies);
            
            WriteIntArray(lengths);

            descriptor.Climb();
        }

        [ProcessorMethod]
        public void ProcessEnumForReadOnly<[IsEnum]TEnum, TUnderlying>
            (IReadOnlyEnumExactValueDescriptor<TEnum, TUnderlying> descriptor)
            where TUnderlying : IConvertible
            where TEnum : IConvertible
        {
            IStateMember underlying = descriptor.UnderlyingValueStateMember;

            descriptor.Route(new BinaryStateMember
                ((IReflectionStateMember) underlying),
                descriptor.Owner,
                true);
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
            WriteAssemblyQualifiedNameIfNeeded(descriptor);

            _writer.Write(descriptor.Get());
        }

        [ProcessorMethod]
        public void ProcessForReadOnly(IReadOnlyValueDescriptor<DateTime> descriptor)
        {
            _writer.Write(descriptor.Get().Ticks);
        }

        public void ProcessNull<TField>(IWriteOnlyValueDescriptor<TField> descriptor)
        {
            _writer.Write(ReadWriteHeader.Null);
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
            _writer.Write(ReadWriteHeader.Revisited);
            _writer.Write(_visitedHash[descriptor.Get()]);
        }
    }
}
