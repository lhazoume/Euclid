using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Euclid.Search
{
    /// <summary>
    /// Class which modelizes a glossary of methods that used binary search algorithm
    /// </summary>
    public static class BinarySearch
    {
        #region methods
        /// <summary>
        /// Binary search to find the first index that satisfying a predicate from the items
        /// </summary>
        /// <typeparam name="T">TYpe of item</typeparam>
        /// <param name="items">Collection of items</param>
        /// <param name="predicate">Predicate</param>
        /// <param name="startRange">The starting index of the range to search</param>
        /// <param name="endRange">The ending index of the range to search</param>
        /// <returns>The first index that satisfying the predicate else -1</returns>
        public static int FindFirstIndexOf<T>(this IReadOnlyList<T> items, Predicate<T> predicate, uint? startRange = null, uint? endRange = null)
        {
            try
            {
                int N = items.Count;
                if (startRange.HasValue && startRange.Value >= items.Count) throw new Exception($"Start range [{startRange}] is superior or equal to the size of the collection [{N}]!");
                if (endRange.HasValue && endRange.Value >= items.Count) throw new Exception($"End range [{endRange}] is superior or equal to the size of the collection [{N}]!");
                if (startRange.HasValue && endRange.HasValue && endRange < startRange) throw new Exception($"Start range [{startRange}] is superior to the end range [{endRange}]!");
                int left = startRange.HasValue ? (int)startRange.Value : 0,
                right = endRange.HasValue ? (int) endRange.Value : N;

                if (predicate(items[left])) return left;
                if (!predicate(items[right])) return -1;

                while (left < right)
                {
                    int mid = (left + right) / 2;

                    if (predicate(items[mid])) right = mid;
                    else left = mid;

                    if (right - left == 1) return right;
                }

                return -1;
            }
            catch(Exception ex) { throw ex; }
        }

        /// <summary>
        /// Binary search to find the first index that satisfying a predicate from the items
        /// </summary>
        /// <typeparam name="T">TYpe of item</typeparam>
        /// <param name="items">Collection of items</param>
        /// <param name="predicate">Predicate</param>
        /// <param name="startRange">The starting index of the range to search</param>
        /// <param name="endRange">The ending index of the range to search</param>
        /// <returns>The first index that satisfying the predicate else -1</returns>
        public static int FindLastIndexOf<T>(this IReadOnlyList<T> items, Predicate<T> predicate, uint? startRange = null, uint? endRange = null)
        {
            try
            {
                int N = items.Count;
                if (startRange.HasValue && startRange.Value >= items.Count) throw new Exception($"Start range [{startRange}] is superior or equal to the size of the collection [{N}]!");
                if (endRange.HasValue && endRange.Value >= items.Count) throw new Exception($"End range [{endRange}] is superior or equal to the size of the collection [{N}]!");
                if (startRange.HasValue && endRange.HasValue && endRange < startRange) throw new Exception($"Start range [{startRange}] is superior to the end range [{endRange}]!");
                int left = startRange.HasValue ? (int)startRange.Value : 0,
                right = endRange.HasValue ? (int)endRange.Value : N;

                if (predicate(items[right])) return right;

                while (left < right)
                {
                    int mid = (left + right) / 2;

                    if (predicate(items[mid])) left = mid;
                    else right = mid;

                    if (right - left == 1)
                        if (predicate(items[left])) return left;
                        else return -1;
                }
                return -1;
            }
            catch (Exception ex) { throw ex; }
        }
        #endregion
    }
}
