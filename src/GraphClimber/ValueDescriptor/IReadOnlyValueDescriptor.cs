namespace GraphClimber
{
    /// <remarks>
    /// Allows processing state member values based on their RuntimeType:
    /// Used mostly for serialization.
    /// </remarks>
    public interface IReadOnlyValueDescriptor<out TRuntime> : IValueDescriptor
    {
        /// <summary>
        /// Gets the current value of the field
        /// </summary>
        /// <returns></returns>
        TRuntime Get();         
    }

    /// <remarks>
    /// Allows processing state member values based on their RuntimeType:
    /// Used mostly for serialization.
    /// </remarks>
    public interface IReadOnlyExactValueDescriptor<TExactRuntime> : IValueDescriptor
    {
        /// <summary>
        /// Gets the current value of the field
        /// </summary>
        /// <returns></returns>
        TExactRuntime Get();
    }
}