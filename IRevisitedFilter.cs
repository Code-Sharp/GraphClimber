namespace GraphClimber
{
    /// <summary>
    /// Defines an operation that filters already visited objects
    /// </summary>
    public interface IRevisitedFilter
    {

        /// <summary>
        /// Returns whether the given object has already been visited
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        bool Visited(object obj);

    }
}