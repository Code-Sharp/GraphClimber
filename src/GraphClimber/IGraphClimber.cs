namespace GraphClimber
{
    public interface IGraphClimber<TProcessor>
    {
        void Climb(object parent, TProcessor processor);
        void Route(IStateMember stateMember, object current, TProcessor processor, bool skipSpecialMethod,
                   int[] elementIndex);
    }
}