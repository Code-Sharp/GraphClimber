namespace GraphClimber
{
    public interface IGraphClimber<TProcessor>
    {
        void Climb(object parent, TProcessor processor);
        void Route(object current, TProcessor processor, bool skipSpecialMethod);
    }
}