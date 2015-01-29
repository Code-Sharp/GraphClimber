using System.Collections.Generic;
using System.Linq;

namespace GraphClimber
{
    internal static class Combinatorics
    {
        public static IEnumerable<IEnumerable<T>> CartesianProduct<T>(IEnumerable<IEnumerable<T>> sets)
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
                    CartesianProduct(sets.Skip(1));

                foreach (IEnumerable<T> set in previousCartesianProduct)
                {
                    foreach (T element in firstSet)
                    {
                        yield return Singletone(element).Concat(set);
                    }
                }
            }
        }

        private static IEnumerable<T> Singletone<T>(T element)
        {
            yield return element;
        }
    }
}