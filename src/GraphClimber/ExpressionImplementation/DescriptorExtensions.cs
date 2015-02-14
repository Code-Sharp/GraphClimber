using System;

namespace GraphClimber
{
    internal class DescriptorExtensions
    {
        public static Type GetDescriptorType(IStateMember member, Type runtimeType)
        {
            if (member.CanRead && member.CanWrite)
            {
                // Assume field type == runtime type
                return typeof(ReadWriteDescriptor<,>).
                    MakeGenericType(member.MemberType, runtimeType);
            }
            else if (member.CanRead)
            {
                return typeof(ReadOnlyDescriptor<,>)
                    .MakeGenericType(member.MemberType, runtimeType);
            }
            else if (member.CanWrite)
            {
                return typeof(WriteOnlyDescriptor<>)
                    .MakeGenericType(runtimeType);
            }
            else
            {
                throw new Exception("Are you kidding me?!");
            }
        }
    }
}