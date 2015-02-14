using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GraphClimber
{
    internal class MethodMapper : IMethodMapper
    {
        public MethodInfo GetMethod(Type processorType, IStateMember member, Type runtimeType)
        {
            Type descriptorType = DescriptorExtensions.GetDescriptorType(member, runtimeType);

            // TODO: First implementation: reimplement this.
            GenericArgumentBinder binder =
                new GenericArgumentBinder
                    (new FallbackToFirstCandidateMethodSelector
                        (new BinderMethodSelector(Type.DefaultBinder)));

            // TODO: Return this after processors are fixed.
            // IEnumerable<Type> typesToSearch = processorType.WithInterfaces();
            IEnumerable<Type> typesToSearch = processorType.AsEnumerable();

            IEnumerable<MethodInfo> methods =
                typesToSearch.SelectMany(x => x.GetMethods())
                    .Where(x => x.IsDefined(typeof (ProcessorMethodAttribute)));

            IEnumerable<IGrouping<int, MethodInfo>> candidates =
                methods.Select(method => Bind(binder, method, descriptorType))
                    .Where(x => x != null)
                    .GroupBy(x => x.GetCustomAttribute<ProcessorMethodAttribute>().Precedence)
                    .OrderBy(x => x.Key);

            IGrouping<int, MethodInfo> maximum = candidates.FirstOrDefault();

            if (maximum.Skip(1).Any())
            {
                throw new Exception("Too many candidates for the processor thingy. Use precedence.");
            }
            else
            {
                MethodInfo method = maximum.FirstOrDefault();

                return method;
            }
        }

        private MethodInfo Bind(GenericArgumentBinder binder, MethodInfo method, Type descriptorType)
        {
            MethodInfo bindedMethod;

            if (binder.TryBind(method, new[] { descriptorType }, out bindedMethod))
            {
                return bindedMethod;
            }

            return null;
        }
    }
}