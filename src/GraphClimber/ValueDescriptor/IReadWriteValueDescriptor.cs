namespace GraphClimber
{
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