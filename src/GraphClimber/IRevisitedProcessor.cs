namespace GraphClimber
{
    /// <summary>
    /// Defines an operation that is called instead of the
    /// normal operation in case that object has already been visited.
    /// </summary>
    public interface IRevisitedProcessor : IRevisitedFilter
    {
        /// <summary>
        /// Called in case that a object is already visited.
        /// </summary>
        /// <typeparam name="TField"></typeparam>
        /// <param name="descriptor"></param>
        void ProcessRevisited<TField>(IReadOnlyValueDescriptor<TField> descriptor);
    }
}