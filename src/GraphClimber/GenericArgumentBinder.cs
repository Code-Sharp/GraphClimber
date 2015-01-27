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
                if (parameterType.IsGenericParameter)
                {
                    var hasStructFlag = parameterType.GenericParameterAttributes.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint);

                    if (hasStructFlag && !realType.IsValueType)
                    {
                        return false;
                    }

                    if (parameterType.GenericParameterAttributes.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint) && !hasStructFlag &&
                        realType.GetConstructor(Type.EmptyTypes) == null)
                    {
                        return false;
                    }

                    if (parameterType.GenericParameterAttributes.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint) && 
                        realType.IsValueType)
                    {
                        return false;
                    }


                    if (parameterType.GetGenericParameterConstraints().Any(constraint => !TryBind(constraint, realType)))
                    {
                        return false;
                    }
                }

                if (parameterType.IsGenericType)
                {
                    var genericParameterType = parameterType.GetGenericTypeDefinition();

                    if (!realType.IsGenericType)
                    {
                        var genericInterface = FindGenericInterface(realType, genericParameterType);

                        if (genericInterface == null)
                        {
                            return false;
                        }

                        realType = genericInterface;
                    }

                    var genericRealType = realType.GetGenericTypeDefinition();

                    if (genericParameterType != genericRealType)
                    {
                        genericRealType = FindGenericInterface(realType, genericParameterType);

                        if (genericRealType == null)
                        {
                            return false;
                        }
                    }

                    var parameterGenericArguments = parameterType.GetGenericArguments();
                    var realGenericArguments = realType.GetGenericArguments();

                    for (int i = 0; i < parameterGenericArguments.Length; i++)
                    {
                        if (!TryBind(parameterGenericArguments[i], realGenericArguments[i]))
                        {
                            return false;
                        }
                    }

                }

                if (parameterType.IsGenericParameter)
                {
                    Type oldRealType = _genericConstraintsToType[parameterType];
                    if (oldRealType != null && oldRealType != realType)
                    {
                        return false;
                    }

                    _genericConstraintsToType[parameterType] = realType;
                }

                return true;
            }

            private static Type FindGenericInterface(Type realType, Type genericParameterType)
            {
                var realInterface =
                    realType.GetInterfaces()
                        .FirstOrDefault(
                            i => i.IsGenericType && i.GetGenericTypeDefinition() == genericParameterType);
                return realInterface;
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
