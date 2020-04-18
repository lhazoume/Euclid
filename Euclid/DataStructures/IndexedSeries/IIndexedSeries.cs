﻿using Euclid.Serialization;
using System;

namespace Euclid.DataStructures.IndexedSeries
{
    /// <summary>
    /// Interface for all series : DataFrame, Slice, Series
    /// </summary>
    /// <typeparam name="T">the type of legends</typeparam>
    /// <typeparam name="TU">the type of the data</typeparam>
    /// <typeparam name="TV">the type of the labels</typeparam>
    public interface IIndexedSeries<T, TU, TV> : IXmlable, ICSVable where T : IComparable<T>, IEquatable<T> where TV : IEquatable<TV>
    {
        /// <summary>Gets the legends</summary>
        T[] Legends { get; }
        /// <summary> Gets the labels </summary>
        TV[] Labels { get; }
        /// <summary>Gets the number of rows</summary>
        int Rows { get; }
        /// <summary>Gets the number of columns </summary>
        int Columns { get; }
        /// <summary>Applies a function to the data</summary>
        /// <param name="function">the function</param>
        void ApplyOnData(Func<TU, TU> function);
    }
}
