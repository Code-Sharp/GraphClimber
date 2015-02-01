namespace GraphClimber
{
    /// <remarks>
    /// Allows processing state member values based on both static and runtime types.
    /// </remarks>
    public interface IReadWriteValueDescriptor<TField> :
        IWriteValueDescriptor<TField>
    {
        /// <summary>
        /// Gets the current value of the field
        /// </summary>
        /// <returns></returns>
        TField Get();
    }
}