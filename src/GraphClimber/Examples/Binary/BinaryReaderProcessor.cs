using System;
using System.Collections.Generic;
using System.IO;
using GraphClimber.Bulks;

namespace GraphClimber.Examples
{
    internal class BinaryReaderProcessor : IWriteOnlyExactPrimitiveProcessor
    {
        private readonly IList<object> _objects = new List<object>();
        private readonly BinaryReader _reader;

        public BinaryReaderProcessor(BinaryReader reader)
        {
            _reader = reader;
        }

        [ProcessorMethod(Precedence = 98)]
        public void ProcessObject(IWriteOnlyExactValueDescriptor<object> descriptor)
        {
            Type type;

            if (TryReadReferenceType(descriptor, out type))
            {
                if (type == typeof (object))
                {
                    descriptor.Set(new object());
                    return;
                }

                descriptor.Route(
                    new BinaryStateMember(
                        new MyCustomStateMember((IReflectionStateMember) descriptor.StateMember, type),
                        true,
                        true),
                    descriptor.Owner);
            }
        }

        [ProcessorMethod(Precedence = 102)]
        public void ProcessStruct<T>(IReadWriteValueDescriptor<T> descriptor)
            where T : struct
        {
            T instance = new T();
            descriptor.Set(instance);
            descriptor.Climb();
        }

        [ProcessorMethod(Precedence = 102)]
        public void ProcessReferenceType<T>(IWriteOnlyExactValueDescriptor<T> descriptor)
            where T : class
        {
            Type type;

            if (TryReadReferenceType(descriptor, out type))
            {
                T instance = (T) Activator.CreateInstance(type);
                _objects.Add(instance);
                descriptor.Set(instance);
                descriptor.Climb();
            }
        }

        private bool TryReadReferenceType<T>(IWriteOnlyExactValueDescriptor<T> descriptor, out Type type)
            where T : class
        {
            BinaryStateMember member = descriptor.StateMember as BinaryStateMember;

            type = member.RuntimeType;

            if (member.HeaderHandled)
            {
                return true;
            }

            byte header = _reader.ReadByte();

            if (header == ReadWriteHeader.Null)
            {
                descriptor.Set(null);
            }
            else if (header == ReadWriteHeader.Revisited)
            {
                HandleVisited(descriptor);
            }
            else
            {
                if (!member.KnownType &&
                    (header == ReadWriteHeader.UnknownType))
                {
                    string assemblyQualifiedName = _reader.ReadString();

                    type = Type.GetType(assemblyQualifiedName);

                    return true;
                }
                else if (header == ReadWriteHeader.KnownType)
                {
                    return true;
                }
                else
                {
                    throw new Exception(
                        "Read an unknown header - probably a mismatch between reader and writer - i.e: a bug :(");
                }
            }

            return false;
        }

        private void HandleVisited<T>(IWriteOnlyExactValueDescriptor<T> descriptor)
        {
            int index = _reader.ReadInt32();
            descriptor.Set((T) _objects[index]);
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
            Type type;

            // Yeah yeah yeah, string is also a reference type.
            if (TryReadReferenceType(descriptor, out type))
            {
                string value = _reader.ReadString();
                _objects.Add(value);
                descriptor.Set(value);
            }
        }

        [ProcessorMethod]
        public void ProcessForWriteOnly(IWriteOnlyExactValueDescriptor<DateTime> descriptor)
        {
            long ticks = _reader.ReadInt64();
            descriptor.Set(DateTime.FromBinary(ticks));
        }

        [ProcessorMethod]
        public void ProcessArray<[IsArray] TArray>(IWriteOnlyExactValueDescriptor<TArray> descriptor)
            where TArray : class
        {
            // TODO: a bit too much code duplication here.
            Type type;

            if (TryReadReferenceType(descriptor, out type))
            {
                int[] lowerIndices = ReadIntArray();
                int[] upperIndices = ReadIntArray();
                Type elementType = typeof (TArray).GetElementType();

                TArray value;

                if (type == elementType.MakeArrayType())
                {
                    // avoid T[*].
                    int length = upperIndices[0] + 1;

                    value =
                        (TArray) (object)
                            Array.CreateInstance(elementType, length);
                }
                else
                {
                    value =
                        (TArray) (object) Array.CreateInstance
                            (elementType,
                                lowerIndices,
                                upperIndices);
                }

                _objects.Add(value);

                descriptor.Set(value);

                descriptor.Climb();
            }
        }

        private int[] ReadIntArray()
        {
            int length = _reader.ReadInt32();

            int[] result = new int[length];

            for (int i = 0; i < result.Length; i++)
            {
                int value = _reader.ReadInt32();
                result[i] = value;
            }

            return result;
        }
    }
}