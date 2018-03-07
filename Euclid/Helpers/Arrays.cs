using System;
using System.Collections.Generic;
using System.Linq;

namespace Euclid.Helpers
{
    /// <summary>
    /// Helper class for generic array manipulations
    /// </summary>
    public static class Arrays
    {
        /// <summary>clones a generic array</summary>
        /// <typeparam name="T">template type</typeparam>
        /// <param name="data">the array of data</param>
        /// <returns>a shallow copy of data</returns>
        public static T[] Clone<T>(this T[] data)
        {
            T[] result = new T[data.Length];
            for (int i = 0; i < data.Length; i++)
                result[i] = data[i];
            return result;
        }

        /// <summary>
        /// clones a generic 2D array
        /// </summary>
        /// <typeparam name="T">template type</typeparam>
        /// <param name="data">the 2d array of data</param>
        /// <returns>a shallow copy of data</returns>
        public static T[,] Clone<T>(this T[,] data)
        {
            int n = data.GetLength(0),
                p = data.GetLength(1);
            T[,] result = new T[n, p];
            for (int i = 0; i < n; i++)
                for (int j = 0; j < p; j++)
                    result[i, j] = data[i, j];
            return result;
        }

        /// <summary>
        /// Extract a sub array of data
        /// </summary>
        /// <typeparam name="T"> template type</typeparam>
        /// <param name="data">the array from which the data will be extracted</param>
        /// <param name="index">the starting index</param>
        /// <param name="length">the number of values to copy</param>
        /// <returns>the extracted array</returns>
        public static T[] SubArray<T>(this T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }

        /// <summary>
        /// Applies a function to all the values in an arry
        /// </summary>
        /// <typeparam name="T">template type</typeparam>
        /// <param name="data">the array to transform</param>
        /// <param name="function">the function to apply to the values</param>
        /// <returns>an array with the transformed data</returns>
        public static T[] Apply<T>(this T[] data, Func<T, T> function)
        {
            T[] result = new T[data.Length];
            for (int i = 0; i < data.Length; i++)
                result[i] = function(data[i]);
            return result;
        }

        public static List<T[]> Split<T>(this T[] data, int size)
        {

            List<T[]> result = new List<T[]>();

            IEnumerable<T> list = data;
            T[] slice = list.Take(size).ToArray();
            list = list.Skip(size);

            while (slice.Length != 0)
            {
                result.Add(slice);
                slice = list.Take(size).ToArray();
                list = list.Skip(size);
            }

            return result;
        }
    }
}
