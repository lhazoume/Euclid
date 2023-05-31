using Euclid.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Euclid.DataStructures.IndexedSeries
{
    /// <summary>
    /// Class representing a serie ordered by time
    /// </summary>
    /// <typeparam name="TU">Data type</typeparam>
    /// <typeparam name="TV">Label type</typeparam>
    public class TimeSeries<TU, TV> : Series<DateTime, TU, TV> where TV : IEquatable<TV>
    {
        #region accessors
        private DateTime[] _timestamps;
        #endregion

        #region constructors
        /// <summary>
        /// Parameterless constructor
        /// </summary>
        protected TimeSeries() { }
        #endregion

        #region methods
        /// <summary>
        /// Initialize serie instance
        /// </summary>
        /// <param name="label">Label</param>
        /// <param name="legends">Legends</param>
        /// <param name="data">Data</param>
        protected override void Initialize(TV label, IList<DateTime> legends, TU[] data)
        {
            _data = Arrays.Clone(data);
            _label = label;
            _legends = new SortedHeader<DateTime>(legends);
            _timestamps = legends.ToArray();
        }
        #endregion

        #region accessors
        /// <summary>
        /// returns the values of legend
        /// </summary>
        public override DateTime[] Legends => _timestamps;
        #endregion
    }
}
