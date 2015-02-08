using System;
using System.Reflection;

namespace GraphClimber
{
    public interface IMethodSelector
    {
        MethodBase Select(MethodBase[] candidates, Type[] realTypes);

    }


    public class BinderMethodSelector : IMethodSelector
    {
        private readonly Binder _binder;

        public BinderMethodSelector(Binder binder)
        {
            _binder = binder;
        }

        public MethodBase Select(MethodBase[] candidates, Type[] realTypes)
        {
            MethodBase result =
                        _binder.SelectMethod(BindingFlags.Instance |
                                                        BindingFlags.Static |
                                                        BindingFlags.Public |
                                                        BindingFlags.NonPublic,
                            candidates,
                            realTypes,
                            new ParameterModifier[0]);

            return result;
        }
    }

    /// <summary>
    /// Sometimes binders having a hard time to decide which method is the best
    /// to use with the given real types.
    /// 
    /// Because the nature of the <see cref="GenericArgumentBinder" /> to give the concrete runtime types
    /// as the first arguments, we select the first candidate when a <see cref="AmbiguousMatchException"/>
    /// is thrown.
    /// </summary>
    public class FallbackToFirstCandidateMethodSelector : IMethodSelector
    {
        private readonly IMethodSelector _child;

        public FallbackToFirstCandidateMethodSelector(IMethodSelector child)
        {
            _child = child;
        }

        public MethodBase Select(MethodBase[] candidates, Type[] realTypes)
        {
            try
            {
                return _child.Select(candidates, realTypes);

            }
            catch (AmbiguousMatchException)
            {
                return candidates[0];
            }

        }
    }
}