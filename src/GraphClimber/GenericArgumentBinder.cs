using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace GraphClimber
{
    public class CartesianProduct
    {
        public static IEnumerable<IEnumerable<T>> GetCartesianProduct<T>(IEnumerable<IEnumerable<T>> sets)
        {
            if (!sets.Skip(1).Any())
            {
                foreach (IEnumerable<T> firstSet in sets.Take(1))
                {
                    foreach (T element in firstSet)
                    {
                        yield return Singletone(element);
                    }
                }
            }

            foreach (IEnumerable<T> firstSet in sets.Take(1))
            {
                IEnumerable<IEnumerable<T>> previousCartesianProduct =
                    GetCartesianProduct(sets.Skip(1));

                foreach (IEnumerable<T> set in previousCartesianProduct)
                {
                    foreach (T element in firstSet)
                    {
                        yield return Singletone(element).Concat(set);
                    }
                }
            }
        }

        public static IEnumerable<T> Singletone<T>(T element)
        {
            yield return element;
        }
    }

    public class GenericArgumentBinder : IGenericArgumentBinder
    {
        private class GenericArgumentBinderContext
        {
            private readonly Type[] _runtimeParameterTypes;
            private readonly MethodInfo _methodInfo;

            private readonly IDictionary<Type, HashSet<Type>> _genericConstraintsToType;

            public GenericArgumentBinderContext(MethodInfo methodInfo, Type[] runtimeParameterTypes)
            {
                _methodInfo = methodInfo;
                _runtimeParameterTypes = runtimeParameterTypes;

                if (methodInfo.ContainsGenericParameters)
                {
                    _genericConstraintsToType =
                        methodInfo.GetGenericArguments()
                            .ToDictionary(type => type, t => new HashSet<Type>());
                }
            }

            public bool TryBind(out IEnumerable<MethodInfo> candidates)
            {
                Type[] staticParameterTypes =
                    _methodInfo.GetParameters()
                        .Select(parameter => parameter.ParameterType).ToArray();

                List<MethodInfo> result = new List<MethodInfo>();
                candidates = result;

                if (staticParameterTypes.Length != _runtimeParameterTypes.Length)
                {
                    return false;
                }

                for (int i = 0; i < staticParameterTypes.Length; i++)
                {
                    Type staticParameterType = staticParameterTypes[i];
                    Type runtimeParameterType = _runtimeParameterTypes[i];

                    if (!TryBind(staticParameterType, runtimeParameterType))
                    {
                        return false;
                    }
                }

                foreach (Type[] candidate in GetGenericArgumentsCandidates())
                {
                    MethodInfo bindedMethod = TryBindMethod(candidate);

                    if (bindedMethod != null)
                    {
                        result.Add(bindedMethod);
                    }
                }

                return result.Count > 0;
            }

            private MethodInfo TryBindMethod(Type[] candidate)
            {
                // TODO: reimplement this.
                try
                {
                    MethodInfo result = _methodInfo.MakeGenericMethod(candidate);
                    return result;
                }
                catch (Exception)
                {
                    return null;
                }
            }

            private IEnumerable<Type[]> GetGenericArgumentsCandidates()
            {
                IEnumerable<IEnumerable<Type>> product =
                    CartesianProduct.GetCartesianProduct
                        (_genericConstraintsToType.Values);

                foreach (IEnumerable<Type> candidate in product)
                {
                    yield return candidate.ToArray();
                }
            }

            private bool TryBind(Type staticType, Type realType)
            {
                if (staticType.IsAssignableFrom(realType))
                {
                    return true;
                }

                bool result = false;

                if (staticType.IsGenericParameter &&
                    VerifyGenericConstraints(staticType, realType))
                {
                    _genericConstraintsToType[staticType].Add(realType);

                    result = true;
                }

                if (staticType.IsGenericType &&
                    VerifyGenericTypesAreCompatible(staticType, realType))
                {
                    result = true;
                }

                if (staticType.IsArray &&
                    VerifyArrayTypesAreCompatiable(staticType, realType))
                {
                    result =  true;
                }

                // Try binding to base types too.
                foreach (Type intefaceType in realType.GetInterfacesAndBase())
                {
                    if (TryBind(staticType, intefaceType))
                    {
                        result = true;
                    }
                }

                return result;
            }

            private bool VerifyGenericTypesAreCompatible(Type genericType, Type realType)
            {
                IEnumerable<Type> implementations =
                    realType
                        .GetClosedGenericTypeImplementation
                        (genericType.GetGenericTypeDefinition());

                bool result = false;

                foreach (Type implementation in implementations)
                {
                    bool isMatched = 
                        VerifyGenericImplementationIsCompatiable(genericType, implementation);

                    if (isMatched)
                    {
                        result = true;
                    }
                }

                return result;
            }

            private bool VerifyGenericImplementationIsCompatiable(Type genericType, Type implementation)
            {
                bool isMatched = true;

                Type[] implementationArguments = implementation.GetGenericArguments();
                Type[] staticArguments = genericType.GetGenericArguments();

                for (int i = 0;
                    i < implementationArguments.Length && isMatched;
                    i++)
                {
                    if (!TryBind(staticArguments[i], implementationArguments[i]))
                    {
                        isMatched = false;
                    }
                }

                return isMatched;
            }

            /// <summary>
            /// Asserts that the given <paramref name="parameterType"/> and
            /// <paramref name="realType"/> are both compatible in case
            /// that they are arrays.
            /// </summary>
            /// <param name="parameterType"></param>
            /// <param name="realType"></param>
            /// <returns></returns>
            private bool VerifyArrayTypesAreCompatiable(Type parameterType, Type realType)
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

            /// <summary>
            /// Asserts generic arguments for special attributes : new(), class, struct
            /// </summary>
            /// <param name="genericParameterType"></param>
            /// <param name="realType"></param>
            /// <returns></returns>
            private bool VerifyGenericConstraints(Type genericParameterType, Type realType)
            {
                bool hasStructFlag =
                    genericParameterType.GenericParameterAttributes.HasFlag
                        (GenericParameterAttributes.NotNullableValueTypeConstraint);

                bool isNonNullableStruct =
                    realType.IsValueType &&
                    !genericParameterType.IsNullable();

                // If the struct flag is not honored
                if (hasStructFlag && !isNonNullableStruct)
                {
                    return false;
                }

                // If the default constructor is not honored : (When struct - constructor is not needed)
                if (genericParameterType.GenericParameterAttributes.HasFlag
                    (GenericParameterAttributes.DefaultConstructorConstraint) &&
                    !(hasStructFlag ||
                    (!realType.IsAbstract &&
                    realType.GetConstructor(Type.EmptyTypes) != null)))
                {
                    return false;
                }

                // If "class" constraint is not honored
                if (genericParameterType.GenericParameterAttributes.HasFlag
                    (GenericParameterAttributes.ReferenceTypeConstraint) &&
                    isNonNullableStruct)
                {
                    return false;
                }

                // Bind parameter type to real type (Hard stuff)
                if (genericParameterType.GetGenericParameterConstraints()
                    .Any(constraint => !TryBind(constraint, realType)))
                {
                    return false;
                }

                return true;
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
                    candidates.Where(x => x.GetGenericArguments().All(y => y != typeof (object)));
 
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

    internal static class TypeExtensions
    {
        public static IEnumerable<Type> GetInterfacesAndBase(this Type type)
        {
            Type baseType = type.BaseType;

            if (baseType != null)
            {
                yield return baseType;
            }

            foreach (Type interfaceType in type.GetInterfaces())
            {
                yield return interfaceType;
            }
        }

        public static bool IsNullable(this Type type)
        {
            return (type.IsGenericType &&
                    (type.GetGenericTypeDefinition() == typeof(Nullable<>)));
        }

        /// <summary>
        /// Returns the closed generic type of the given generic open type,
        /// that the given type is assignable to.
        /// </summary>
        /// <param name="type">The given type.</param>
        /// <param name="openGenericType">The open generic type.</param>
        /// <returns>The closed generic type of the given generic open type,
        /// that the given type is assignable to.</returns>
        public static IEnumerable<Type> GetClosedGenericTypeImplementation(this Type type, Type openGenericType)
        {
            if (type == null)
            {
                return Enumerable.Empty<Type>();
            }

            if (type.IsGenericType &&
                type.GetGenericTypeDefinition() == openGenericType)
            {
                return Singletone(type);
            }

            if (!openGenericType.IsInterface)
            {
                return type.BaseType.GetClosedGenericTypeImplementation(openGenericType);
            }
            else
            {
                return type.GetInterfaces()
                    .SelectMany(x => GetClosedGenericTypeImplementation(x, openGenericType));
            }
        }

        private static IEnumerable<T> Singletone<T>(T element)
        {
            yield return element;
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