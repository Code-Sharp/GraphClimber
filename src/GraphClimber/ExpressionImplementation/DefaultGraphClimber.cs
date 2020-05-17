namespace GraphClimber
{
    public class DefaultGraphClimber<TProcessor> : IGraphClimber<TProcessor>
    {
        private readonly ClimbStore _climbStore;

        private DefaultGraphClimber(IStateMemberProvider stateMemberProvider, IMethodMapper methodMapper)
        {
            _climbStore = new ClimbStore(typeof (TProcessor), stateMemberProvider, methodMapper);
        }

        public void Climb(object parent, TProcessor processor)
        {
            _climbStore.GetClimb<object>(parent.GetType())(processor, parent);
        }

        public void Route(IStateMember stateMember, object current, TProcessor processor, bool skipSpecialMethod,
                          int[] elementIndex)
        {
            _climbStore.GetRoute(stateMember, current.GetType())(processor, current, skipSpecialMethod, elementIndex);
        }

        public static IGraphClimber<TProcessor> Create(IStateMemberProvider stateMemberProvider)
        {
            return new DefaultGraphClimber<TProcessor>(
                stateMemberProvider,
                new MethodMapper());
        } 
    }
}
