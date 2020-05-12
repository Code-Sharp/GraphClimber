using System;
using System.Collections.Generic;
using System.Linq;
using GraphClimber.Bulks;
using GraphClimber.Examples.Binary;

namespace GraphClimber.Examples
{
    public static class ReadWriteHeader
    {
        public const byte Null = 1;
        public const byte Revisited = 2;
        public const byte KnownType = 3;
        public const byte UnknownType = 4;
    }

    class ReferenceEqualityComparer : IEqualityComparer<object>
    {
        bool IEqualityComparer<object>.Equals(object x, object y)
        {
            return ReferenceEquals(x, y);
        }

        int IEqualityComparer<object>.GetHashCode(object obj)
        {
            if (obj == null)
            {
                return 0;
            }

            return obj.GetHashCode();
        }
    }

    class BinaryWriterProcessor : IReadOnlyExactPrimitiveProcessor, INullProcessor, IRevisitedProcessor
    {
        private readonly IWriter _writer;

        private readonly IDictionary<object, int> _visitedHash = 
            new Dictionary<object, int>(new ReferenceEqualityComparer());

        public BinaryWriterProcessor(IWriter writer)
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
            WriteAssemblyQualifiedNameIfNeeded(descriptor);
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

        private void WriteAssemblyQualifiedNameIfNeeded<T>(IReadOnlyExactValueDescriptor<T> descriptor)
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
        public void ProcessArray<[IsArray]TArray>(IReadOnlyExactValueDescriptor<TArray> descriptor)
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

        [ProcessorMethod(Precedence = 96)]
        public void ProcessEnumForReadOnly<[IsEnum]TEnum, TUnderlying>
            (IReadOnlyEnumExactValueDescriptor<TEnum, TUnderlying> descriptor)
            where TUnderlying : IConvertible
            where TEnum : IConvertible
        {
            WriteAssemblyQualifiedNameIfNeeded((BinaryStateMember)descriptor.StateMember, typeof (TEnum));

            IStateMember underlying = descriptor.UnderlyingValueStateMember;

            descriptor.Route(new BinaryStateMember(underlying, true, true),
                descriptor.Owner,
                true);
        }

        [ProcessorMethod]
        public void ProcessForReadOnly(IReadOnlyExactValueDescriptor<byte> descriptor)
        {
            WriteAssemblyQualifiedNameIfNeeded(descriptor);
            _writer.Write(descriptor.Get());
        }

        [ProcessorMethod]
        public void ProcessForReadOnly(IReadOnlyExactValueDescriptor<sbyte> descriptor)
        {
            WriteAssemblyQualifiedNameIfNeeded(descriptor);
            _writer.Write(descriptor.Get());
        }

        [ProcessorMethod]
        public void ProcessForReadOnly(IReadOnlyExactValueDescriptor<short> descriptor)
        {
            WriteAssemblyQualifiedNameIfNeeded(descriptor);
            _writer.Write(descriptor.Get());
        }

        [ProcessorMethod]
        public void ProcessForReadOnly(IReadOnlyExactValueDescriptor<ushort> descriptor)
        {
            WriteAssemblyQualifiedNameIfNeeded(descriptor);
            _writer.Write(descriptor.Get());
        }

        [ProcessorMethod]
        public void ProcessForReadOnly(IReadOnlyExactValueDescriptor<int> descriptor)
        {
            WriteAssemblyQualifiedNameIfNeeded(descriptor);
            _writer.Write(descriptor.Get());
        }

        [ProcessorMethod]
        public void ProcessForReadOnly(IReadOnlyExactValueDescriptor<uint> descriptor)
        {
            WriteAssemblyQualifiedNameIfNeeded(descriptor);
            _writer.Write(descriptor.Get());
        }

        [ProcessorMethod]
        public void ProcessForReadOnly(IReadOnlyExactValueDescriptor<long> descriptor)
        {
            WriteAssemblyQualifiedNameIfNeeded(descriptor);
            _writer.Write(descriptor.Get());
        }

        [ProcessorMethod]
        public void ProcessForReadOnly(IReadOnlyExactValueDescriptor<ulong> descriptor)
        {
            WriteAssemblyQualifiedNameIfNeeded(descriptor);
            _writer.Write(descriptor.Get());
        }

        [ProcessorMethod]
        public void ProcessForReadOnly(IReadOnlyExactValueDescriptor<char> descriptor)
        {
            WriteAssemblyQualifiedNameIfNeeded(descriptor);
            _writer.Write(descriptor.Get());
        }

        [ProcessorMethod]
        public void ProcessForReadOnly(IReadOnlyExactValueDescriptor<double> descriptor)
        {
            WriteAssemblyQualifiedNameIfNeeded(descriptor);
            _writer.Write(descriptor.Get());
        }

        [ProcessorMethod]
        public void ProcessForReadOnly(IReadOnlyExactValueDescriptor<string> descriptor)
        {
            WriteAssemblyQualifiedNameIfNeeded(descriptor);

            _writer.Write(descriptor.Get());
        }

        [ProcessorMethod]
        public void ProcessForReadOnly(IReadOnlyExactValueDescriptor<DateTime> descriptor)
        {
            WriteAssemblyQualifiedNameIfNeeded(descriptor);
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

            if (BinaryReaderWriterExtensions.IsPredefinedType(obj.GetType()))
            {
                return false;
            }

            _visitedHash[obj] = _visitedHash.Count;

            return false;
        }

        public void ProcessRevisited<TField>(IReadWriteValueDescriptor<TField> descriptor)
        {
            _writer.Write(ReadWriteHeader.Revisited);
            _writer.Write(_visitedHash[descriptor.Get()]);
        }
    }
}
