using System;

namespace GraphClimber
{

    /// <summary>
    /// Defines an actor that can process values that are 
    /// of type <typeparamref name="TField"/> (Without descendents, oppose to <see cref="IInhreitedProcessor{TField}"/>)
    /// </summary>
    /// <typeparam name="TField"></typeparam>
    public interface IProcessor<TField>
    {

        /// <summary>
        /// A method that will be called when a value of type <typeparamref name="TField"/>
        /// is found within the graph climbed.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="descriptor"></param>
        void Process(TField value, IValueDescriptor<TField> descriptor);

    }

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


    /// <summary>
    /// Defines an actor that processes all the .net primitives.
    /// </summary>
    public interface IPrimitiveProcessor : 
        IProcessor<byte>, IProcessor<sbyte>,
        IProcessor<short>, IProcessor<ushort>,
        IProcessor<int>, IProcessor<uint>,
        IProcessor<long>, IProcessor<ulong>,
        IProcessor<char>, IProcessor<double>,
        IProcessor<string>, IProcessor<DateTime>
    {

    }
}