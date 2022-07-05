using Euclid.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Euclid.DataStructures.IndexedSeries
{
    public class TimeSeries<TU, TV> : Series<DateTime, TU, TV> where TV : IEquatable<TV>
    {
        #region accessors
        private DateTime[] _timestamps;
        #endregion

        #region constructors
        /// <summary>
        /// Parameterless constructor
        /// </summary>
        public TimeSeries() { }
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
        }
        #endregion
    }
}
