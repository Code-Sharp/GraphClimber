using GraphClimber.ExpressionCompiler;

namespace GraphClimber
{
    class DefaultGraphClimber<TProcessor> : IGraphClimber<TProcessor>
    {
        private readonly ClimbStore _climbStore;

        public DefaultGraphClimber(IStateMemberProvider stateMemberProvider, IMethodMapper methodMapper, IExpressionCompiler expressionCompiler )
        {
            _climbStore = new ClimbStore(typeof (TProcessor), stateMemberProvider, methodMapper, expressionCompiler);
        }

        public void Climb(object parent, TProcessor processor)
        {
            _climbStore.GetClimb<object>(parent.GetType())(processor, parent);
        }

        public void Route(IStateMember stateMember, object current, TProcessor processor, bool skipSpecialMethod)
        {
            _climbStore.GetRoute(stateMember, current.GetType())(processor, current, skipSpecialMethod);
        }

        public static IGraphClimber<TProcessor> Create(IStateMemberProvider stateMemberProvider)
        {
            return new DefaultGraphClimber<TProcessor>(
                stateMemberProvider,
                new MethodMapper(),
                new TrivialExpressionCompiler());
        } 
    }
}
