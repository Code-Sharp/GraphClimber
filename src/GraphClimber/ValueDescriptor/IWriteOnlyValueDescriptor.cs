namespace GraphClimber
{
    /// <remarks>
    /// Allows processing state member values based on their static type:
    /// Used mostly for deserialization.
    /// </remarks>
    /// TODO: Not sure if needed, maybe exact value descriptor is enough?
    public interface IWriteOnlyValueDescriptor<in TField> : IValueDescriptor
    {
        /// <summary>
        /// Sets the field to a new value
        /// </summary>
        /// <param name="value"></param>
        void Set(TField value);
    }

    /// <remarks>
    /// Allows processing state member values based on their static type:
    /// Used mostly for deserialization.
    /// </remarks>
    public interface IWriteOnlyExactValueDescriptor<TExactField> : IValueDescriptor
    {
        /// <summary>
        /// Sets the field to a new value
        /// </summary>
        /// <param name="value"></param>
        void Set(TExactField value);
    }
}