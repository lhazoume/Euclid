﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace Euclid.Extensions
{
    /// <summary>Helper class for generic array manipulations</summary>
    public static class Arrays
    {
        /// <summary>Returns a jagged array of the right type</summary>
        /// <typeparam name="T">the underlying type</typeparam>
        /// <param name="rows">the array's number of rows</param>
        /// <param name="columns">the array's number of columuns</param>
        /// <returns>a jagged array</returns>
        public static T[][] Build<T>(int rows, int columns)
        {
            T[][] result = new T[rows][];
            for (int i = 0; i < rows; i++)
                result[i] = new T[columns];
            return result;
        }


        /// <summary>Clones a jagged array</summary>
        /// <typeparam name="T">template type</typeparam>
        /// <param name="data">the 2d array of data</param>
        /// <returns>a shallow copy of data</returns>
        public static T[][] Clone<T>(this T[][] data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            int n = data.Length;
            T[][] result = new T[n][];
            for (int i = 0; i < n; i++)
            {
                result[i] = new T[data[i].Length];
                for (int j = 0; j < data[i].Length; j++)
                    result[i][j] = data[i][j];
            }

            return result;
        }

        /// <summary>clones a generic array</summary>
        /// <typeparam name="T">template type</typeparam>
        /// <param name="data">the array of data</param>
        /// <returns>a shallow copy of data</returns>
        public static T[] Clone<T>(this T[] data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            T[] result = new T[data.Length];
            for (int i = 0; i < data.Length; i++)
                result[i] = data[i];
            return result;
        }

        /// <summary>Clones a generic 2D array</summary>
        /// <typeparam name="T">template type</typeparam>
        /// <param name="data">the 2d array of data</param>
        /// <returns>a shallow copy of data</returns>
        public static T[,] Clone<T>(this T[,] data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            int n = data.GetLength(0),
                p = data.GetLength(1);
            T[,] result = new T[n, p];
            for (int i = 0; i < n; i++)
                for (int j = 0; j < p; j++)
                    result[i, j] = data[i, j];
            return result;
        }

        /// <summary>Clones a generic 3D array</summary>
        /// <typeparam name="T">template type</typeparam>
        /// <param name="data">the 2d array of data</param>
        /// <returns>a shallow copy of data</returns>
        public static T[,,] Clone<T>(this T[,,] data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            int n = data.GetLength(0),
                p = data.GetLength(1),
                q = data.GetLength(2);
            T[,,] result = new T[n, p, q];
            for (int i = 0; i < n; i++)
                for (int j = 0; j < p; j++)
                    for (int k = 0; k < q; k++)
                        result[i, j, k] = data[i, j, k];
            return result;
        }

        /// <summary>Extract a sub array of data</summary>
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

        /// <summary>Applies a function to all the values in an arrzy</summary>
        /// <typeparam name="T">template type</typeparam>
        /// <param name="data">the array to transform</param>
        /// <param name="function">the function to apply to the values</param>
        /// <returns>an array with the transformed data</returns>
        public static T[] Apply<T>(this T[] data, Func<T, T> function)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            T[] result = new T[data.Length];
            for (int i = 0; i < data.Length; i++)
                result[i] = function(data[i]);
            return result;
        }

        /// <summary>Returns the index of the nearest value in the array</summary>
        /// <param name="data">the array to search</param>
        /// <param name="value">the sought value</param>
        /// <returns>the index in the array</returns>
        public static int IndexOfNearest(this double[] data, double value)
        {
            if (data.Length == 0) throw new ArgumentOutOfRangeException(nameof(data));
            int nearestIndex = 0;
            double shortestDistance = Math.Abs(value - data[nearestIndex]);

            for (int i = 1; i < data.Length; i++)
            {
                double currentDistance = Math.Abs(value - data[i]);
                if (currentDistance < shortestDistance)
                {
                    nearestIndex = i;
                    shortestDistance = currentDistance;
                }
            }

            return nearestIndex;
        }

        /// <summary>Splits an array into smaller size arrays</summary>
        /// <typeparam name="T">the generic type</typeparam>
        /// <param name="data">the array to split</param>
        /// <param name="size">the target size for the smaller arrays</param>
        /// <returns>a List of arrays</returns>
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

        /// <summary>Find indices according to a predicate</summary>
        /// <typeparam name="T">the item type</typeparam>
        /// <param name="items">the candidates</param>
        /// <param name="predicate">the predicate</param>
        /// <returns>the matching indices</returns>
        public static IEnumerable<int> FindIndices<T>(this IEnumerable<T> items, Func<T, bool> predicate)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));

            int i = 0;

            foreach (var item in items)
            {
                if (predicate(item))
                    yield return i;
                i++;
            }
        }

        /// <summary>Checks if an array is sorted</summary>
        /// <typeparam name="T">the template type</typeparam>
        /// <param name="data">the input array</param>
        /// <returns><c>true</c> is the array is sorted, <c>false</c> otherwise</returns>
        public static bool IsSorted<T>(this T[] data) where T : IComparable<T>
        {
            for (int i = 1; i < data.Length; i++)
                if (data[i - 1].CompareTo(data[i]) > 0)
                    return false;
            return true;
        }
    }
}
