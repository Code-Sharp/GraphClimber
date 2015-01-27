namespace GraphClimber
{


    /// <summary>
    /// Represents a description for a value of type
    /// <typeparamref name="TField"/>
    /// </summary>
    /// <typeparam name="TField"></typeparam>
    public interface IValueDescriptor<TField>
    {

        /// <summary>
        /// Gets the state member that owns the value
        /// </summary>
        IStateMember StateMember { get; }

        /// <summary>
        /// Sets the field to a new value
        /// </summary>
        /// <param name="value"></param>
        void Set(TField value);

        /// <summary>
        /// Gets the current value of the field
        /// </summary>
        /// <returns></returns>
        TField Get();

        /// <summary>
        /// Gets the owner of the field
        /// </summary>
        object Owner { get; }

        /// <summary>
        /// Climbs on the (current) value that found on the owner.
        /// </summary>
        void Climb();

    }
}