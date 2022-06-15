using Euclid.Extensions;
using Euclid.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Euclid.DataStructures.IndexedSeries
{
    /// <summary>Class representing a DataFrame of synchronized data</summary>
    /// <typeparam name="T">the legend type</typeparam>
    /// <typeparam name="TU">the data type</typeparam>
    /// <typeparam name="TV">the label type</typeparam>
    public class DataFrame<T, TU, TV> : IDataFrame<T, TU, TV> where T : IComparable<T>, IEquatable<T> where TV : IEquatable<TV>
    {
        #region Constructors
        /// <summary>
        /// Hidden constructor
        /// </summary>
        /// <param name="labels">header of labels</param>
        /// <param name="legends">header of legends</param>
        /// <param name="data">data</param>
        protected DataFrame(IHeader<TV> labels, IHeader<T> legends, TU[,] data) : base(labels, legends, data) { }

        /// <summary>
        /// public constructor
        /// </summary>
        protected DataFrame() { }

        /// <summary>
        /// Initialize DataFrame instance
        /// </summary>
        /// <param name="labels">labels</param>
        /// <param name="legends">legends</param>
        /// <param name="data">data</param>
        protected override void Initialize(IList<TV> labels, IList<T> legends, TU[,] data)
        {
            _data = Arrays.Clone(data);
            _labels = new Header<TV>(labels);
            _legends = new Header<T>(legends);
        }
        #endregion

        #region methods
        #endregion
}
}
