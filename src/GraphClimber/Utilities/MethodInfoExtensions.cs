using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GraphClimber
{
    internal static class MethodInfoExtensions
    {
        public static bool TryMakeGenericMethodMethod(this MethodInfo methodInfo, Type[] typeArguments,
            out MethodInfo bindedMethod)
        {
            Type[] genericArguments = methodInfo.GetGenericArguments();

            bindedMethod = null;

            if ((genericArguments.Length == 0) && (typeArguments.Length == 0))
            {
                bindedMethod = methodInfo;
                return true;
            }

            if (genericArguments.Length != typeArguments.Length)
            {
                return false;
            }

            if (!ValidateGeneralGenericConstraints(genericArguments, typeArguments))
            {
                return false;
            }

            if (!IsGenericArgumentsSubstitutionCompatible(genericArguments, typeArguments))
            {
                return false;
            }

            bindedMethod = methodInfo.MakeGenericMethod(typeArguments);
            return true;
        }

        private static bool IsGenericArgumentsSubstitutionCompatible(Type[] genericArguments, Type[] substitution)
        {
            Dictionary<Type, Type> currentMap =
                genericArguments.Zip(substitution,
                    Tuple.Create)
                    .ToDictionary(x => x.Item1, x => x.Item2);

            ArgumentSubstituter substituter = new ArgumentSubstituter(currentMap);

            foreach (Type argument in genericArguments)
            {
                Type[] constraints = argument.GetGenericParameterConstraints();

                if (constraints.Length > 0)
                {
                    Type resolvedArgument = substituter.Substite(argument);

                    foreach (Type constraint in constraints)
                    {
                        Type resolvedConstraint = substituter.Substite(constraint);

                        if (!resolvedConstraint.IsAssignableFrom(resolvedArgument))
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        private static bool ValidateGeneralGenericConstraints(Type[] genericArguments, Type[] typeArguments)
        {
            for (int i = 0; i < genericArguments.Length; i++)
            {
                Type genericArgument = genericArguments[i];
                Type typeArgument = typeArguments[i];

                if (!ValidateParameterTypeGeneralGenericConstraints(genericArgument, typeArgument))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool ValidateParameterTypeGeneralGenericConstraints(Type genericParameterType, Type realType)
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
                !(realType.IsValueType ||
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

            return true;
        }

        private class ArgumentSubstituter
        {
            private readonly IDictionary<Type, Type> _map;

            public ArgumentSubstituter(IDictionary<Type, Type> map)
            {
                _map = map;
            }

            public Type Substite(Type type)
            {
                Type result;

                if (type.IsGenericParameter)
                {
                    result = _map[type];
                }
                else if (type.IsGenericType)
                {
                    Type[] arguments =
                        type.GetGenericArguments()
                            .Select(x => Substite(x)).ToArray();

                    result =
                        type.GetGenericTypeDefinition().MakeGenericType(arguments);
                }
                else if (type.IsArray)
                {
                    Type elementType =
                        Substite(type.GetElementType());

                    result = elementType.MakeArrayType(type.GetArrayRank());
                }
                else
                {
                    result = type;
                }

                return result;
            }
        }
    }
}