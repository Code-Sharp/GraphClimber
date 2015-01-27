using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GraphClimber
{
    public class GenericArgumentBinder : IGenericArgumentBinder
    {

        class GenericArgumentBinderContext
        {
            private readonly Type[] _realTypes;
            private readonly MethodInfo _methodInfo;

            private readonly IDictionary<Type, Type> _genericConstraintsToType;

            public GenericArgumentBinderContext(MethodInfo methodInfo, Type[] realTypes)
            {
                _methodInfo = methodInfo;
                _realTypes = realTypes;

                if (methodInfo.ContainsGenericParameters)
                {
                    _genericConstraintsToType = methodInfo.GetGenericArguments().ToDictionary(t => t, t => (Type)null);
                }
            }

            public bool TryBind(out MethodInfo bindedMethod)
            {
                var parameterTypes = _methodInfo.GetParameters().Select(p => p.ParameterType).ToArray();

                bindedMethod = null;

                if (parameterTypes.Length != _realTypes.Length)
                {
                    return false;
                }

                for (int i = 0; i < parameterTypes.Length; i++)
                {
                    var parameterType = parameterTypes[i];
                    var realType = _realTypes[i];

                    if (!TryBind(parameterType, realType))
                    {
                        return false;
                    }

                }

                bindedMethod = _methodInfo.MakeGenericMethod(_genericConstraintsToType.Values.ToArray());
                return true;
            }


            private bool TryBind(Type parameterType, Type realType)
            {
                if (parameterType.IsGenericParameter && !AssertGenericParameterAttributes(parameterType, realType))
                {
                    return false;
                }

                if (!AssertGenericTypes(parameterType, ref realType))
                {
                    return false;
                }

                if (!AssertGenericParameter(parameterType, realType))
                {
                    return false;
                }

                if (!AssertArrayTypes(parameterType, realType))
                {
                    return false;
                }

                if (!parameterType.IsGenericParameter && 
                    !parameterType.IsGenericType &&
                    !parameterType.IsArray &&  
                    parameterType != realType)
                {
                    return false;
                }

                return true;
            }

            /// <summary>
            /// Asserts and returnes whether the given to types has matching
            /// type with matching generic arguments
            /// </summary>
            /// <param name="parameterType"></param>
            /// <param name="realType"></param>
            /// <returns></returns>
            private bool AssertGenericTypes(Type parameterType, ref Type realType)
            {
                if (parameterType.IsGenericType)
                {
                    var genericParameterType = parameterType.GetGenericTypeDefinition();

                    IEnumerable<Type> realTypes = new[] {realType};

                    if (!realType.IsGenericType)
                    {
                        var genericInterface = FindGenericInterface(realType, genericParameterType).ToList();

                        if (!genericInterface.Any())
                        {
                            return false;
                        }

                        realTypes = genericInterface;
                    }

                    foreach (var possibleRealType in realTypes)
                    {
                        if (GenericBind(parameterType, possibleRealType, genericParameterType))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                return true;
            }

            private bool GenericBind(Type parameterType, Type possibleRealType, Type genericParameterType)
            {
                var genericRealType = possibleRealType.GetGenericTypeDefinition();

                if (genericParameterType != genericRealType)
                {
                    genericRealType = FindGenericInterface(possibleRealType, genericParameterType).FirstOrDefault();

                    if (genericRealType == null)
                    {
                        return false;
                    }
                }

                var parameterGenericArguments = parameterType.GetGenericArguments();
                var realGenericArguments = possibleRealType.GetGenericArguments();

                for (int i = 0; i < parameterGenericArguments.Length; i++)
                {
                    if (!TryBind(parameterGenericArguments[i], realGenericArguments[i]))
                    {
                        return false;
                    }
                }

                return true;
            }

            /// <summary>
            /// Checkes whether the generic parameter type
            /// given is not setted to other real type that is not as the
            /// given <paramref name="realType"/>
            /// </summary>
            /// <param name="parameterType"></param>
            /// <param name="realType"></param>
            /// <returns></returns>
            private bool AssertGenericParameter(Type parameterType, Type realType)
            {
                if (!parameterType.IsGenericParameter)
                {
                    return true;
                }

                Type oldRealType = _genericConstraintsToType[parameterType];
                if (oldRealType != null && oldRealType != realType)
                {
                    return false;
                }

                _genericConstraintsToType[parameterType] = realType;

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
            private bool AssertArrayTypes(Type parameterType, Type realType)
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
            /// <param name="parameterType"></param>
            /// <param name="realType"></param>
            /// <returns></returns>
            private bool AssertGenericParameterAttributes(Type parameterType, Type realType)
            {
                var hasStructFlag =
                    parameterType.GenericParameterAttributes.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint);

                // If the struct flag is not honored
                if (hasStructFlag && !realType.IsValueType)
                {
                    return false;
                }

                // If the default constructor is not honored : (When struct - constructor is not needed)
                if (parameterType.GenericParameterAttributes.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint) &&
                    !hasStructFlag &&
                    realType.GetConstructor(Type.EmptyTypes) == null)
                {
                    return false;
                }

                // If "class" constraint is not honored
                if (parameterType.GenericParameterAttributes.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint) &&
                    realType.IsValueType)
                {
                    return false;
                }

                // Bind parameter type to real type (Hard stuff)
                if (parameterType.GetGenericParameterConstraints().Any(constraint => !TryBind(constraint, realType)))
                {
                    return false;
                }

                return true;
            }

            /// <summary>
            /// Finds an implemented (generic) interface in <paramref name="realType"/>
            /// that matches the generic interface given in <paramref name="genericInterfaceType"/>
            /// </summary>
            /// <param name="realType"></param>
            /// <param name="genericInterfaceType"></param>
            /// <returns></returns>
            private static IEnumerable<Type> FindGenericInterface(Type realType, Type genericInterfaceType)
            {
                return realType.GetInterfaces()
                        .Where(
                            i => i.IsGenericType && i.GetGenericTypeDefinition() == genericInterfaceType);
            }
        }

        public bool TryBind(MethodInfo methodInfo, Type[] realTypes, out MethodInfo bindedMethod)
        {
            return new GenericArgumentBinderContext(methodInfo, realTypes).TryBind(out bindedMethod);
        }

    }

    public interface IGenericArgumentBinder
    {

        bool TryBind(MethodInfo methodInfo, Type[] realTypes, out MethodInfo bindedMethod);

    }
}
