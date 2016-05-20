using System.Collections.Generic;
using System.Linq;

namespace Euclid.Arithmetics
{
    /// <summary>
    /// Subsets class
    /// </summary>
    public static class Subsets
    {
        /// <summary>
        /// Returns all possibles subsets of a List
        /// </summary>
        /// <typeparam name="T">the template type</typeparam>
        /// <param name="data">the original data</param>
        /// <returns>the List of subsets</returns>
        public static List<List<T>> SubSets<T>(List<T> data)
        {
            if (data.Count() == 0)
                return new List<List<T>>() { new List<T>() };

            List<T> element = data.Take(1).ToList();

            List<List<T>> haveNots = SubSets(data.Skip(1).ToList()),
                haves = haveNots.Select(set => element.Concat(set).ToList()).ToList();

            return haves.Concat(haveNots).ToList();
        }
    }
}
