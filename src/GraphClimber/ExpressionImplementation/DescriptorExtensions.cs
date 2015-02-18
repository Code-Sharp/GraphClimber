using System;

namespace GraphClimber
{
    internal class DescriptorExtensions
    {
        public static Type GetDescriptorType(IStateMember member, Type runtimeType)
        {
            if (member.CanRead && member.CanWrite)
            {
                // We're really screwed up here, how are we going to support everything in the world?
                if (member.OwnerType.IsValueType)
                {
                    return typeof (StructReadWriteDescriptor<,,>)
                        .MakeGenericType(member.OwnerType, member.MemberType, runtimeType);
                }

                if (runtimeType.IsEnum)
                {
                    return typeof (EnumReadWriteDescriptor<,,>)
                        .MakeGenericType(member.MemberType, runtimeType, runtimeType.GetEnumUnderlyingType());
                }

                // Assume field type == runtime type
                return typeof(ReadWriteDescriptor<,>).
                    MakeGenericType(member.MemberType, runtimeType);
            }
            if (member.CanRead)
            {
                if (runtimeType.IsEnum)
                {
                    throw new NotImplementedException("Descriptor for enum readonly is not yet available");
                }

                return typeof(ReadOnlyDescriptor<,>)
                    .MakeGenericType(member.MemberType, runtimeType);
            }
            
            if (member.CanWrite)
            {
                if (runtimeType.IsEnum)
                {
                    throw new NotImplementedException("Descriptor for enum write only is not yet available");
                }

                return typeof(WriteOnlyDescriptor<>)
                    .MakeGenericType(runtimeType);
            }
            
            throw new Exception("Are you kidding me?!");
        }
    }
}