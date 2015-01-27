namespace GraphClimber
{
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
}