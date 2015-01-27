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
        /// <typeparam name="TReal"></typeparam>
        /// <param name="value"></param>
        /// <param name="descriptor"></param>
        void ProcessRevisited<TField, TReal>(TReal value, IValueDescriptor<TField> descriptor)
            where TReal : TField;

    }
}