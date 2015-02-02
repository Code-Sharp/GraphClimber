namespace GraphClimber
{
    /// <summary>
    /// Defines an actor that can process read values that are 
    /// of type <typeparamref name="TField"/> or descendents
    /// of that type.
    /// </summary>
    /// <typeparam name="TField"></typeparam>
    public interface IReadExactProcessor<in TField>
    {
        void ProcessForRead(IReadOnlyValueDescriptor<TField> descriptor);
    }

    /// <summary>
    /// Defines an actor that can process values that are 
    /// of static type <typeparamref name="TField"/> or less 
    /// </summary>
    /// <typeparam name="TField"></typeparam>
    /// <remarks>
    /// Example: Set all fields that their static type is 
    /// assignable from IEnumerable{char} to string.Empty.
    /// This includes object and IEnumerable, IEnumerable{string}.
    /// </remarks>
    public interface IWriteProcessor<TField>
    {
        void ProcessForWrite(IWriteOnlyValueDescriptor<TField> descriptor);
    }

    /// <summary>
    /// Defines an actor that can process values that are 
    /// of both runtime and static type <typeparamref name="TField"/>.
    /// </summary>
    /// <typeparam name="TField"></typeparam>
    public interface IReadWriteProcessor<TField>
    {
        void ProcessForReadWrite(IReadWriteValueDescriptor<TField> descriptor);
    }
}