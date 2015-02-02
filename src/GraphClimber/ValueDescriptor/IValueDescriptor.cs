using System;

namespace GraphClimber
{
    /// <summary>
    /// Represents a description for a value of type
    /// </summary>
    public interface IValueDescriptor
    {
        /// <summary>
        /// Gets the state member that owns the value
        /// </summary>
        IStateMember StateMember { get; }

        /// <summary>
        /// Gets the owner of the field
        /// </summary>
        object Owner { get; }

        /// <summary>
        /// Climbs on the (current) value that found in the field by the owner
        /// </summary>
        /// <param name="processor"></param>
        void Climb(object processor);

        void Route(IStateMember stateMember, Type runtimeMemberType, object owner, object processor);

        void Route(IStateMember stateMember, object owner, object processor);
    }
}