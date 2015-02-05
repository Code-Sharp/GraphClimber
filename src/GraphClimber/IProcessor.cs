namespace GraphClimber
{

    /// <summary>
    /// Defines an actor that can process values that are 
    /// of type <typeparamref name="TField"/> (Without descendents, oppose to <see cref="IInheritedProcessor{TField}"/>)
    /// </summary>
    /// <typeparam name="TField"></typeparam>
    public interface IProcessor<TField>
    {
        /// <summary>
        /// A method that will be called when a value of type <typeparamref name="TField"/>
        /// is found within the graph climbed.
        /// </summary>
        /// <param name="descriptor"></param>
        [ProcessorMethod]
        void ProcessForReadWrite(IReadWriteValueDescriptor<TField> descriptor);
    }

    /// <summary>
    /// Defines an actor that can process values that are 
    /// of type <typeparamref name="TField"/> (With descendents, oppose to <see cref="IReadOnlyExactProcessor{TField}"/>)
    /// </summary>
    /// <typeparam name="TField"></typeparam>
    public interface IReadOnlyProcessor<in TField>
    {
        /// <summary>
        /// A method that will be called when a value of type <typeparamref name="TField"/>
        /// is found within the graph climbed.
        /// </summary>
        /// <param name="descriptor"></param>
        [ProcessorMethod]
        void ProcessForReadOnly(IReadOnlyValueDescriptor<TField> descriptor);
    }

    /// <summary>
    /// Defines an actor that can process values that are 
    /// of type <typeparamref name="TField"/> (Without descendents, oppose to <see cref="IInheritedProcessor{TField}"/>)
    /// </summary>
    /// <typeparam name="TField"></typeparam>
    public interface IReadOnlyExactProcessor<TField>
    {
        /// <summary>
        /// A method that will be called when a value of type <typeparamref name="TField"/>
        /// is found within the graph climbed.
        /// </summary>
        /// <param name="descriptor"></param>
        [ProcessorMethod]
        void ProcessForReadOnly(IReadOnlyExactValueDescriptor<TField> descriptor);
    }


    /// <summary>
    /// Defines an actor that can process values that are 
    /// of type <typeparamref name="TField"/> (Without descendents, oppose to <see cref="IInheritedProcessor{TField}"/>)
    /// </summary>
    /// <typeparam name="TField"></typeparam>
    public interface IWriteOnlyExactProcessor<TField>
    {
        /// <summary>
        /// A method that will be called when a value of type <typeparamref name="TField"/>
        /// is found within the graph climbed.
        /// </summary>
        /// <param name="descriptor"></param>
        [ProcessorMethod]
        void ProcessForWriteOnly(IWriteOnlyExactValueDescriptor<TField> descriptor);
    }
}