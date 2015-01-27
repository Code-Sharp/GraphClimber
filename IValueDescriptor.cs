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
        /// Climbs on the (current) value that found on the owner.
        /// </summary>
        void Climb();
    }

    /// <remarks>
    /// Allows processing state member values based on their RuntimeType:
    /// Used mostly for serialization.
    /// </remarks>
    public interface IReadValueDescriptor<out TRuntime> : IValueDescriptor
    {
        /// <summary>
        /// Gets the current value of the field
        /// </summary>
        /// <returns></returns>
        TRuntime Get();         
    }

    /// <remarks>
    /// Allows processing state member values based on their static type:
    /// Used mostly for deserialization.
    /// </remarks>
    public interface IWriteValueDescriptor<in TField> : IValueDescriptor
    {
        /// <summary>
        /// Sets the field to a new value
        /// </summary>
        /// <param name="value"></param>
        void Set(TField value);         
    }

    /// <remarks>
    /// Allows processing state member values based on both static and runtime types.
    /// </remarks>
    /// <example>
    /// IReadWriteValueDescriptor{IEnumerable{string}, ICollection{string}} represents
    /// state members that have static type that is at most IEnumerable{string}, but 
    /// runtime type that is at least ICollection{string}.
    /// </example>
    public interface IReadWriteValueDescriptor<out TField, in TRuntime> :
        IReadValueDescriptor<TField>, IWriteValueDescriptor<TRuntime>
    {
    }

    /// <remarks>
    /// Allows processing state member values based on both static and runtime types.
    /// </remarks>
    /// <example>
    /// IReadWriteValueDescriptor{IEnumerable{string}, ICollection{string}} represents
    /// state members that have static type that is at most IEnumerable{string}, but 
    /// runtime type that is at least ICollection{string}.
    /// </example>
    public interface IReadWriteValueDescriptor<TField> :
        IReadWriteValueDescriptor<TField, TField>
    {
    }
}