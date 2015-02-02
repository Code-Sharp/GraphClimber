namespace GraphClimber
{
    public interface INullProcessor
    {
        void ProcessNull<TField>(IWriteOnlyValueDescriptor<TField> descriptor);
    }
}