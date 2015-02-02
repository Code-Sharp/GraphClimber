namespace GraphClimber
{
    /// <remarks>
    /// Allows processing state member values based on both static and runtime types.
    /// </remarks>
    public interface IReadWriteValueDescriptor<TField>
    {
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
    }
}