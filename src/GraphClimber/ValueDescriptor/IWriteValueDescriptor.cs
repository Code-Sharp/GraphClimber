namespace GraphClimber
{
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
}