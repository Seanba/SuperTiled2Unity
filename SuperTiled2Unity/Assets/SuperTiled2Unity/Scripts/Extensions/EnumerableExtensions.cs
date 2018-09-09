using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperTiled2Unity
{
    public static class EnumerableExtensions
    {
        public static bool IsEmpty<T>(this IEnumerable<T> array)
        {
            if (array == null)
            {
                return true;
            }

            return !array.Any();
        }
    }
}
