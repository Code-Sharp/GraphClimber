using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace GraphClimber
{
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
        /// <param name="boundMethod">The specific generic method if exists</param>
        /// <returns>Success / failure due to incompatible arguments</returns>
        bool TryBind(MethodInfo methodInfo, Type[] realTypes, out MethodInfo boundMethod);

    }

    public class ArgsMapper
    {
        /// <summary>
        /// Recieves the input parameter types and returns a lookup of potential generic arg mapping
        /// </summary>
        /// <param name="openMethodParamters"></param>
        /// <param name="closedMethodParameters"></param>
        /// <returns></returns>
        public ILookup<Type, Type> MapGenericArgs(IEnumerable<Type> openMethodParamters, IEnumerable<Type> closedMethodParameters)
        {
            var genericArgToRealType =
                openMethodParamters.Zip(closedMethodParameters, GetGenericArgFromParameterType)
                    .SelectMany(x => x)
                    .ToList();

            //Checking for generic args that have generic arg constraints
            MapGenericGenericArgs(genericArgToRealType);

            var finalMapping = genericArgToRealType.ToLookup(x => x.GenericType, x => x.RealType);
            return finalMapping;
        }

        private void MapGenericGenericArgs(List<TypeMapping> genericArgToRealType)
        {
            var genericGenericArgsToRealType = genericArgToRealType;
            do
            {
                genericGenericArgsToRealType =
                    (from mapping in genericGenericArgsToRealType
                     where mapping != null
                     from constraint in mapping.GenericType.GetGenericParameterConstraints()
                     from cnstrMapping in GetGenericArgFromParameterType(constraint, mapping.RealType)
                     select cnstrMapping).Except<TypeMapping>(genericArgToRealType).ToList();

                genericArgToRealType.AddRange(genericGenericArgsToRealType);
            } while (genericGenericArgsToRealType.Any());
        }

        private IEnumerable<TypeMapping> GetGenericArgFromParameterType(Type openType, Type closedType)
        {
            if (openType.IsGenericParameter)
            {
                yield return new TypeMapping(openType, closedType);
                if (IsArgCovariant(openType))
                {
                    foreach (var baseType in closedType.GetBaseTypes())
                    {
                        yield return new TypeMapping(openType, baseType);
                    }
                }
            }

            if (openType.IsArray)
            {
                if (closedType.IsArray && closedType.GetArrayRank() == openType.GetArrayRank())
                {
                    foreach (var elementTypeArgs in GetGenericArgFromParameterType(openType.GetElementType(), closedType.GetElementType()))
                    {
                        yield return elementTypeArgs;
                    }
                }
            }

            else if (openType.IsGenericType)
            {
                var realArgGroups =
                    GetGenericPotentialMatches(openType, closedType).SelectMany(GetGenericParameterPermutations);
                var genericArgMappings =
                    (from argGroup in realArgGroups
                     from argMapping in openType.GetGenericArguments().Zip(argGroup, GetGenericArgFromParameterType)
                     select argMapping).ToList();


                if (genericArgMappings.Any())
                {
                    foreach (var argMapping in genericArgMappings.SelectMany(x => x))
                    {
                        yield return argMapping;
                    }
                }
            }
        }

        private IEnumerable<IEnumerable<Type>> GetGenericParameterPermutations(IEnumerable<Type> openGenericArgs,
            IEnumerable<Type> closedGenericArgs)
        {
            bool[] nonCovariantArgs = openGenericArgs.Select(IsArgCovariant).ToArray();

            var argsWithBaseTypes = closedGenericArgs.Select((arg, i) => nonCovariantArgs[i] ? arg.GetTypeWithBaseTypes() : new[] { arg });

            return Combinatorics.CartesianProduct(argsWithBaseTypes);
        }

        private bool IsArgCovariant(Type typeArg)
        {
            //return true;
            return typeArg.GenericParameterAttributes.HasFlag(GenericParameterAttributes.Covariant);
        }

        private IEnumerable<IEnumerable<Type>> GetGenericParameterPermutations(Type genericType)
        {
            return GetGenericParameterPermutations(genericType.GetGenericTypeDefinition().GetGenericArguments(),
                genericType.GetGenericArguments());
        }

        private static IEnumerable<Type> GetGenericPotentialMatches(Type parameterType, Type realType)
        {
            var genericDefinition = parameterType.GetGenericTypeDefinition();
            var typeToCompare = realType.GetTypeWithBaseTypes()
                    .Where(t => t.IsGenericType && t.GetGenericTypeDefinition() == genericDefinition);
            return typeToCompare;
        }
    }

    public class TypeMapping
    {
        private readonly Type mGenericType;
        private readonly Type mRealType;

        public TypeMapping(Type genericType, Type realType)
        {
            mGenericType = genericType;
            mRealType = realType;
        }

        public Type GenericType
        {
            get { return mGenericType; }
        }

        public Type RealType
        {
            get { return mRealType; }
        }

        protected bool Equals(TypeMapping other)
        {
            return mGenericType == other.mGenericType && mRealType == other.mRealType;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TypeMapping)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((mGenericType != null ? mGenericType.GetHashCode() : 0) * 397) ^ (mRealType != null ? mRealType.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            return String.Format("{{{0}, {1}}}", GenericType, RealType);
        }
    }

    public class GenericArgumentBinder : IGenericArgumentBinder
    {
        private readonly ArgsMapper _argsMapper = new ArgsMapper();

        /// <summary>
        /// Tries to create a specific generic instance of the given <paramref name="methodInfo"/>
        /// using the <paramref name="realTypes"/> of the arguments that are used for the call.
        /// </summary>
        /// <param name="methodInfo">The generic method info</param>
        /// <param name="realTypes">The types of the arguments that used to call the method</param>
        /// <param name="boundMethod">The specific generic method if exists</param>
        /// <returns>Success / failure due to incompatible arguments</returns>
        public bool TryBind(MethodInfo methodInfo, Type[] realTypes, out MethodInfo boundMethod)
        {
            MethodInfo[] candidates;

            bool anyResults =
                TryBind(methodInfo, realTypes, out candidates);

            if (!anyResults)
            {
                boundMethod = null;
            }
            else
            {
                MethodInfo[] filtered = candidates.Where(x => x.GetGenericArguments()
                                                    .All(y => y != typeof(object))).ToArray();

                // Prefer avoiding object as a generic type.
                if (filtered.Any())
                {
                    candidates = filtered;
                }

                if (!TryCollapseMethods(candidates, out boundMethod))
                {
                    MethodBase result =
                        Type.DefaultBinder.SelectMethod(BindingFlags.Instance |
                                                        BindingFlags.Static |
                                                        BindingFlags.Public |
                                                        BindingFlags.NonPublic,
                            candidates,
                            realTypes,
                            new ParameterModifier[0]);
                    boundMethod = (MethodInfo)result;
                }
            }

            return boundMethod != null;
        }

        public bool TryBind(MethodInfo methodInfo, Type[] realTypes, out MethodInfo[] boundMethod)
        {
            boundMethod = GetBoundInfos(methodInfo, realTypes).Distinct().ToArray();
            return boundMethod.Any();
        }

        /// <summary>
        /// Returns all the possible close typed <see cref="MethodInfo"/>s
        /// </summary>
        /// <param name="openTypedMethod"></param>
        /// <param name="realTypes"></param>
        /// <returns></returns>
        private IEnumerable<MethodInfo> GetBoundInfos(MethodInfo openTypedMethod, Type[] realTypes)
        {
            var paramters = openTypedMethod.GetParameters();
            if (paramters.Length != realTypes.Length)
            {
                return Enumerable.Empty<MethodInfo>();
            }

            var finalMapping = _argsMapper.MapGenericArgs(paramters.Select(p => p.ParameterType), realTypes);

            //It means that some generic args cannot be inferred from the paramters
            if (finalMapping.Count != openTypedMethod.GetGenericArguments().Length)
            {
                return Enumerable.Empty<MethodInfo>();
            }

            var mappedArgs = openTypedMethod.GetGenericArguments().Select(x => finalMapping[x].Distinct().ToList()).ToList();

            IEnumerable<IEnumerable<Type>> product =
                Combinatorics.CartesianProduct(mappedArgs);

            return product.Select(p => MakeGeneric(openTypedMethod, p)).Where(p => p != null);
        }

        private static MethodInfo MakeGeneric(MethodInfo methodInfo, IEnumerable<Type> map)
        {
            MethodInfo bindedMethod;
            if (methodInfo.TryMakeGenericMethodMethod(map.ToArray(), out bindedMethod))
            {
                return bindedMethod;
            }
            return null;
        }


        /// <summary>
        /// Tries to find the best matching method out of all the closed type method info candidates
        /// </summary>
        /// <param name="candidates">An enumerable set of candidate methods</param>
        /// <param name="boundMethod">The resulting method</param>
        /// <returns>True if found only one matching method</returns>
        private bool TryCollapseMethods(IEnumerable<MethodInfo> candidates, out MethodInfo boundMethod)
        {
            bool isFail = false;
            var args = candidates.Aggregate((prev, curr) =>
            {
                var prevArgs = prev.GetGenericArguments();
                var currArgs = curr.GetGenericArguments();
                if (AreAssignableFrom(prevArgs, currArgs))
                {
                    return curr;
                }
                if (AreAssignableFrom(currArgs, prevArgs))
                {
                    return prev;
                }
                isFail = true;
                return curr;
            });
            boundMethod = args;
            return !isFail;
        }

        private static bool AreAssignableFrom(IEnumerable<Type> prevArgs, IEnumerable<Type> currArgs)
        {
            return prevArgs.Zip(currArgs, (prevArg, currArg) => prevArg.IsAssignableFrom(currArg)).All(x => x);
        }
    }
}