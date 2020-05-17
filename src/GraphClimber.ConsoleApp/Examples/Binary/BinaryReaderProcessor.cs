using System;
using System.Collections.Generic;
using GraphClimber.Bulks;
using GraphClimber.Examples.Binary;

namespace GraphClimber.Examples
{
    internal class BinaryReaderWriterExtensions
    {
        public static bool IsPredefinedType(Type type)
        {
            return type.IsPrimitive || type.IsEnum ||
                   type == typeof(string) || type == typeof(DateTime) ||
                   type.IsArray;
        }
    }

    internal class BinaryReaderProcessor : IWriteOnlyExactPrimitiveProcessor
    {
        private readonly IList<object> _objects = new List<object>();
        private readonly IReader _reader;

        public BinaryReaderProcessor(IReader reader)
        {
            _reader = reader;
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
                // Do not create instance when predefined type, Just route to it.
                if (BinaryReaderWriterExtensions.IsPredefinedType(type) ||
                    type.IsArray)
                {
                    descriptor.Route(
                        new BinaryStateMember
                            (new MyCustomStateMember(descriptor.StateMember,
                                type),
                                true,
                                true),
                        descriptor.Owner,
                        true);

                    return;
                }

                T instance = (T) Activator.CreateInstance(type);
                _objects.Add(instance);
                descriptor.Set(instance);
                descriptor.Climb();
            }
        }

        private bool TryReadReferenceType<T>(IWriteOnlyExactValueDescriptor<T> descriptor, out Type type)
            where T : class
        {
            BinaryStateMember member = (BinaryStateMember) descriptor.StateMember;

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
                
                if (header == ReadWriteHeader.KnownType)
                {
                    return true;

                }
                
                throw new Exception(
                    "Read an unknown header - probably a mismatch between reader and writer - i.e: a bug :(");
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
            AssertKnownType(descriptor);
            descriptor.Set(_reader.ReadByte());
        }

        private void AssertKnownType(IValueDescriptor descriptor)
        {
            var binaryStateMember = descriptor.StateMember as BinaryStateMember;

            if (binaryStateMember != null && binaryStateMember.HeaderHandled)
            {
                return;
            }

            if (_reader.ReadByte() != ReadWriteHeader.KnownType)
            {
                throw new Exception("Header should have been known.");
            }
        }

        [ProcessorMethod]
        public void ProcessForWriteOnly(IWriteOnlyExactValueDescriptor<sbyte> descriptor)
        {
            AssertKnownType(descriptor);
            descriptor.Set(_reader.ReadSByte());
        }

        [ProcessorMethod]
        public void ProcessForWriteOnly(IWriteOnlyExactValueDescriptor<short> descriptor)
        {
            AssertKnownType(descriptor);
            descriptor.Set(_reader.ReadInt16());
        }

        [ProcessorMethod]
        public void ProcessForWriteOnly(IWriteOnlyExactValueDescriptor<ushort> descriptor)
        {
            AssertKnownType(descriptor);
            descriptor.Set(_reader.ReadUInt16());
        }

        [ProcessorMethod]
        public void ProcessForWriteOnly(IWriteOnlyExactValueDescriptor<int> descriptor)
        {
            AssertKnownType(descriptor);
            descriptor.Set(_reader.ReadInt32());
        }

        [ProcessorMethod]
        public void ProcessForWriteOnly(IWriteOnlyExactValueDescriptor<uint> descriptor)
        {
            AssertKnownType(descriptor);
            descriptor.Set(_reader.ReadUInt32());
        }

        [ProcessorMethod]
        public void ProcessForWriteOnly(IWriteOnlyExactValueDescriptor<long> descriptor)
        {
            AssertKnownType(descriptor);
            descriptor.Set(_reader.ReadInt64());
        }

        [ProcessorMethod]
        public void ProcessForWriteOnly(IWriteOnlyExactValueDescriptor<ulong> descriptor)
        {
            AssertKnownType(descriptor);
            descriptor.Set(_reader.ReadUInt64());
        }

        [ProcessorMethod]
        public void ProcessForWriteOnly(IWriteOnlyExactValueDescriptor<char> descriptor)
        {
            AssertKnownType(descriptor);
            descriptor.Set(_reader.ReadChar());
        }

        [ProcessorMethod]
        public void ProcessForWriteOnly(IWriteOnlyExactValueDescriptor<double> descriptor)
        {
            AssertKnownType(descriptor);
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
                //_objects.Add(value);
                descriptor.Set(value);
            }
        }

        [ProcessorMethod]
        public void ProcessForWriteOnly(IWriteOnlyExactValueDescriptor<DateTime> descriptor)
        {
            AssertKnownType(descriptor);
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
                int[] lengths = ReadIntArray();
                Type elementType = typeof (TArray).GetElementType();

                TArray value;

                if (type == elementType.MakeArrayType())
                {
                    // avoid T[*].
                    int length = lengths[0];

                    value =
                        (TArray) (object)
                            Array.CreateInstance(elementType, length);
                }
                else
                {
                    value =
                        (TArray) (object) Array.CreateInstance
                            (elementType,
                                lengths,
                                lowerIndices);
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

        [ProcessorMethod]
        public void ProcessEnumForWriteOnly<[IsEnum] TEnum, TUnderlying>
            (IEnumReadWriteExactValueDescriptor<TEnum, TUnderlying> descriptor)
            where TUnderlying : IConvertible
            where TEnum : Enum, IConvertible
        {
            IStateMember underlying = descriptor.UnderlyingValueStateMember;

            bool handled = false;

            var olderStateMember = descriptor.StateMember as BinaryStateMember;
            if (olderStateMember != null && olderStateMember.HeaderHandled)
            {
                handled = true;
            }

            descriptor.Route(new BinaryStateMember(underlying, true, handled),
                descriptor.Owner,
                true);
        }
    }
}