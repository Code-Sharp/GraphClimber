using System;

namespace GraphClimber
{
    internal class DescriptorExtensions
    {
        public static Type GetDescriptorType(IStateMember member, Type runtimeType)
        {
            if (member.CanRead && member.CanWrite)
            {
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