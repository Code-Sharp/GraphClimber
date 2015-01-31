using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace GraphClimber
{
    public class GenericArgumentBinder : IGenericArgumentBinder
    {
        private class ArgumentMapper
        {
            private readonly IDictionary<Type, HashSet<Type>> _genericConstraintsToType;

            public ArgumentMapper(MethodInfo methodInfo)
            {
                if (methodInfo.ContainsGenericParameters)
                {
                    _genericConstraintsToType =
                        methodInfo.GetGenericArguments()
                            .ToDictionary(type => type, t => new HashSet<Type>());
                }
            }

            /// <summary>
            /// This method tries to find available bindings
            /// from the given <paramref name="parameterType"/>
            /// to the given <paramref name="realType"/>.
            /// </summary>
            /// <remarks>
            /// This method mutates the return value of <see cref="GetGenericArgumentsCandidates"/>,
            /// So we have to get inside all the available paths, That's why return value isn't returned at
            /// the first place, and only in the end of the method.
            /// </remarks>
            /// <param name="parameterType"></param>
            /// <param name="realType"></param>
            /// <returns></returns>
            public bool TryBind(Type parameterType, Type realType)
            {
                if (parameterType.IsAssignableFrom(realType))
                {
                    return true;
                }

                bool result = false;

                if (parameterType.IsGenericParameter &&
                    VerifyGenericConstraints(parameterType, realType))
                {
                    _genericConstraintsToType[parameterType].Add(realType);
                    result = true;
                }

                if (parameterType.IsGenericType &&
                    VerifyGenericTypesAreCompatible(parameterType, realType))
                {
                    result = true;
                }

                if (parameterType.IsArray &&
                    VerifyArrayTypesAreCompatible(parameterType, realType))
                {
                    result = true;
                }

                // Try binding to base types too.
                foreach (Type intefaceType in realType.GetInterfacesAndBase())
                {
                    if (TryBind(parameterType, intefaceType))
                    {
                        result = true;
                    }
                }

                return result;
            }


            /// <summary>
            /// Asserts generic arguments for special attributes : new(), class, struct
            /// </summary>
            /// <param name="genericParameterType"></param>
            /// <param name="realType"></param>
            /// <returns></returns>
            private bool VerifyGenericConstraints(Type genericParameterType, Type realType)
            {
                // Bind constraints to real type 
                // (Some generic parameters appear only as constraints)
                return genericParameterType.GetGenericParameterConstraints()
                    .All(constraint => TryBind(constraint, realType));
            }

            private bool VerifyGenericTypesAreCompatible(Type genericType, Type realType)
            {
                IEnumerable<Type> implementations =
                    realType
                        .GetClosedGenericTypeImplementation
                        (genericType.GetGenericTypeDefinition());

                return
                    implementations.Where(implementation => VerifyGenericImplementationIsCompatible(genericType, implementation))
                        .ToList()
                        .Any();
            }

            private bool VerifyGenericImplementationIsCompatible(Type genericType, Type implementation)
            {
                Type[] implementationArguments = implementation.GetGenericArguments();
                Type[] staticArguments = genericType.GetGenericArguments();

                for (int i = 0; i < implementationArguments.Length; i++)
                {
                    if (!TryBind(staticArguments[i], implementationArguments[i]))
                    {
                        return false;
                    }
                }

                return true;
            }

            /// <summary>
            /// Asserts that the given <paramref name="parameterType"/> and
            /// <paramref name="realType"/> are both compatible in case
            /// that they are arrays.
            /// </summary>
            /// <param name="parameterType"></param>
            /// <param name="realType"></param>
            /// <returns></returns>
            private bool VerifyArrayTypesAreCompatible(Type parameterType, Type realType)
            {
                if (parameterType.IsArray && realType.IsArray)
                {
                    if (parameterType.GetArrayRank() != realType.GetArrayRank())
                    {
                        return false;
                    }

                    if (!TryBind(parameterType.GetElementType(), realType.GetElementType()))
                    {
                        return false;
                    }
                }

                return true;
            }

            public IEnumerable<Type[]> GetGenericArgumentsCandidates()
            {
                IEnumerable<IEnumerable<Type>> product =
                    Combinatorics.CartesianProduct(_genericConstraintsToType.Values);

                foreach (IEnumerable<Type> candidate in product)
                {
                    yield return candidate.ToArray();
                }
            }
        }

        private class GenericArgumentBinderContext
        {
            private readonly Type[] _runtimeParameterTypes;
            private readonly MethodInfo _methodInfo;

            public GenericArgumentBinderContext(MethodInfo methodInfo, Type[] runtimeParameterTypes)
            {
                _methodInfo = methodInfo;
                _runtimeParameterTypes = runtimeParameterTypes;
            }

            public bool TryBind(out IEnumerable<MethodInfo> candidates)
            {
                Type[] parameterTypes =
                    _methodInfo.GetParameters()
                        .Select(parameter => parameter.ParameterType)
                        .ToArray();

                List<MethodInfo> result = new List<MethodInfo>();
                candidates = result;

                if (parameterTypes.Length != _runtimeParameterTypes.Length)
                {
                    return false;
                }

                ArgumentMapper mapper = new ArgumentMapper(_methodInfo);

                for (int i = 0; i < parameterTypes.Length; i++)
                {
                    Type parameterType = parameterTypes[i];
                    Type runtimeParameterType = _runtimeParameterTypes[i];

                    if (!mapper.TryBind(parameterType, runtimeParameterType))
                    {
                        return false;
                    }
                }

                foreach (Type[] candidate in mapper.GetGenericArgumentsCandidates())
                {
                    MethodInfo bindedMethod;

                    if (_methodInfo.TryMakeGenericMethodMethod(candidate, out bindedMethod))
                    {
                        result.Add(bindedMethod);
                    }
                }

                return result.Count > 0;
            }
        }

        public bool TryBind(MethodInfo methodInfo, Type[] realTypes, out MethodInfo[] bindedMethods)
        {
            IEnumerable<MethodInfo> candidates;

            bool anyResults =
                new GenericArgumentBinderContext(methodInfo, realTypes).TryBind(out candidates);

            bindedMethods = candidates.ToArray();

            return anyResults;
        }

        public bool TryBind(MethodInfo methodInfo, Type[] realTypes, out MethodInfo bindedMethod)
        {
            MethodInfo[] candidates;

            bool anyResults =
                TryBind(methodInfo, realTypes, out candidates);

            if (!anyResults)
            {
                bindedMethod = null;
            }
            else
            {
                IEnumerable<MethodInfo> filtered =
                    candidates.Where(x => x.GetGenericArguments()
                        .All(y => y != typeof(object)));

                // Prefer avoiding object as a generic type.
                if (filtered.Any())
                {
                    candidates = filtered.ToArray();
                }

                MethodBase result =
                    Type.DefaultBinder.SelectMethod(BindingFlags.Instance |
                                                    BindingFlags.Static |
                                                    BindingFlags.Public |
                                                    BindingFlags.NonPublic,
                        candidates,
                        realTypes,
                        new ParameterModifier[0]);

                bindedMethod = (MethodInfo)result;
            }

            return anyResults;
        }
    }

    /// <summary>
    /// When calling a generic method using <see cref="MethodCallExpression" />,
    /// It is not enough to put the arguments to the call, We also need to create a specific 
    /// generic method for the given arguments.
    /// 
    /// This interface creates the generic method for the given arguments.
    /// </summary>
    public interface IGenericArgumentBinder
    {
        /// <summary>
        /// Tries to create a specific generic instance of the given <paramref name="methodInfo"/>
        /// using the <paramref name="realTypes"/> of the arguments that are used for the call.
        /// </summary>
        /// <param name="methodInfo">The generic method info</param>
        /// <param name="realTypes">The types of the arguments that used to call the method</param>
        /// <param name="bindedMethod">The specific generic method if exists</param>
        /// <returns>Success / failure due to incompatible arguments</returns>
        bool TryBind(MethodInfo methodInfo, Type[] realTypes, out MethodInfo bindedMethod);
    }
}