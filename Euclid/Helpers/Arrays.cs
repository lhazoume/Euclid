using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Euclid.Helpers
{
    public static class Arrays
    {
        public static T[] Clone<T>(this T[] data)
        {
            T[] result = new T[data.Length];
            for (int i = 0; i < data.Length; i++)
                result[i] = data[i];
            return result;
        }

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

        public static T[] SubArray<T>(this T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }

        public static T[] Apply<T>(this T[] data, Func<T, T> function)
        {
            T[] result = new T[data.Length];
            for (int i = 0; i < data.Length; i++)
                result[i] = function(data[i]);
            return result;
        }
    }
}
