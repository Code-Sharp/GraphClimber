using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GraphClimber
{
    internal static class MethodInfoExtensions
    {
        public static bool TryMakeGenericMethodMethod(this MethodInfo methodInfo, Type[] genericArguments,
            out MethodInfo bindedMethod)
        {
            if (!IsGenericArgumentsSubstituteCompatible(methodInfo, genericArguments))
            {
                bindedMethod = null;
                return false;
            }
            else
            {
                bindedMethod = methodInfo.MakeGenericMethod(genericArguments);
                return true;
            }
        }

        private static bool IsGenericArgumentsSubstituteCompatible(MethodInfo methodInfo, Type[] candidate)
        {
            Type[] genericArguments = methodInfo.GetGenericArguments();

            Dictionary<Type, Type> currentMap =
                genericArguments.Zip(candidate,
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