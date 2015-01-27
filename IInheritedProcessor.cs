namespace GraphClimber
{
    /// <summary>
    /// Defines an actor that can process values that are 
    /// of type <typeparamref name="TField"/> or descendents
    /// of that type.
    /// </summary>
    /// <typeparam name="TField"></typeparam>
    public interface IInheritedProcessor<TField>
    {
        void Process<TReal>(TReal value, IValueDescriptor<TField> descriptor)
            where TReal : TField;

    }
}