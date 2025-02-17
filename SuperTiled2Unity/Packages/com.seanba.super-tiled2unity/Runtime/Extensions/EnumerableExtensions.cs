using System;
using System.Collections.Generic;
using System.Linq;

namespace SuperTiled2Unity
{
    public static class EnumerableExtensions
    {
        public static int GetOrderIndependentHashCode<T>(this IEnumerable<T> source)
        {
            int hash = 0;
            foreach (T element in source)
            {
                hash ^= EqualityComparer<T>.Default.GetHashCode(element);
            }

            return hash;
        }

        public static bool IsEmpty<T>(this IEnumerable<T> array)
        {
            if (array == null)
            {
                return true;
            }

            return !array.Any();
        }

        public static Dictionary<TKey, TElement> SafeToDictionary<TSource, TKey, TElement>(
             this IEnumerable<TSource> source,
             Func<TSource, TKey> keySelector,
             Func<TSource, TElement> elementSelector,
             IEqualityComparer<TKey> comparer = null)
        {
            var dictionary = new Dictionary<TKey, TElement>(comparer);

            if (source == null)
            {
                return dictionary;
            }

            foreach (TSource element in source)
            {
                dictionary[keySelector(element)] = elementSelector(element);
            }

            return dictionary;
        }
    }
}
