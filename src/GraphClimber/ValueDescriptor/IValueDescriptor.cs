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
        void Climb();

        // TODO: remove this
        void Reprocess(Type staticMemberType);
    }
}