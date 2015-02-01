namespace GraphClimber
{
    public interface INullProcessor
    {
        void ProcessNull<TField>(IWriteValueDescriptor<TField> descriptor);
    }
}