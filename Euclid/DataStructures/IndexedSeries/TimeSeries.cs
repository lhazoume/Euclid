using Euclid.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Euclid.DataStructures.IndexedSeries
{
    /// <summary>
    /// Class representing a serie ordered by time
    /// </summary>
    /// <typeparam name="TU"></typeparam>
    /// <typeparam name="TV"></typeparam>
    public class TimeSeries<TU, TV> where TV : IEquatable<TV> // : Series<DateTime, TU, TV>
    {
        #region Variables
        private TV _label;
        private TU[] _data;
        private SortedHeader<DateTime> _legends;
        #endregion


        #region Constructors
        /// <summary> Builds a <c>TimeSeries</c></summary>
        /// <param name="label">Label</param>
        /// <param name="legends">Legends</param>
        /// <param name="data">Data</param>
        private TimeSeries(TV label, IList<DateTime> legends, TU[] data)
        {
            _data = Arrays.Clone(data);
            _label = label;
            _legends = new SortedHeader<DateTime>(legends);
        }
        #endregion

        #region methods

        #endregion

        #region Accessors
        /// <summary>Returns the values of legend</summary>
        public  DateTime[] Legends => _legends.Values;
        #endregion
    }
}
