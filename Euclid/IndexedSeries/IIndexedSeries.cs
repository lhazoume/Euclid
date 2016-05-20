using Euclid.Serialization;
using System;

namespace Euclid.IndexedSeries
{
    /// <summary>
    /// Interface for all series : DataFrame, Slice, Series
    /// </summary>
    /// <typeparam name="T">the type of legends</typeparam>
    /// <typeparam name="U">the type of the data</typeparam>
    /// <typeparam name="V">the type of the labels</typeparam>
    public interface IIndexedSeries<T, U, V> : IXmlable, ICSVable where T : IComparable<T>, IEquatable<T> where V : IEquatable<V>
    {
        /// <summary>Gets the legends</summary>
        T[] Legends { get; }
        /// <summary> Gets the labels </summary>
        V[] Labels { get; }
        /// <summary>Gets the number of rows</summary>
        int Rows { get; }
        /// <summary>Gets the number of columns </summary>
        int Columns { get; }
        /// <summary>Applies a function to the data</summary>
        /// <param name="function">the function</param>
        void ApplyOnData(Func<U, U> function);
    }
}
