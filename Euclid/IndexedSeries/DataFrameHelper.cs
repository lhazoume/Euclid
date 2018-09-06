using System;
using System.Collections.Generic;

namespace Euclid.IndexedSeries
{
    public static class DataFrameHelper
    {
        /// <summary>
        /// Find indices according a predicate
        /// </summary>
        /// <typeparam name="T">item type</typeparam>
        /// <param name="items">collection of candidates</param>
        /// <param name="predicate">condition expression</param>
        /// <returns>Matching indices</returns>
        public static IEnumerable<int> FindIndices<T>(this IEnumerable<T> items, Func<T, bool> predicate)
        {
            int i = 0;

            foreach (var item in items)
            {
                if (predicate(item))
                    yield return i;
                i++;
            }
        }
    }
}
