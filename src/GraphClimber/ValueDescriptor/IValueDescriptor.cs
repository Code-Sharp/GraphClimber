using System;
using System.Collections.Generic;

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
        /// Gets the children of the current state member. Useful for forward-only readers.
        /// </summary>
        IReadOnlyDictionary<string, IStateMember> Children { get; }

        /// <summary>
        /// Gets the owner of the field
        /// </summary>
        object Owner { get; }

        /// <summary>
        /// Climbs on the (current) value that found in the field by the owner
        /// </summary>
        void Climb();

        void Route(IStateMember stateMember, Type runtimeMemberType, object owner, bool skipSpecialMethod);

        void Route(IStateMember stateMember, object owner, bool skipSpecialMethod);
    }
}