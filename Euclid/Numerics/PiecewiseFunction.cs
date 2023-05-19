using Euclid.Histograms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace Euclid.Numerics
{
    /// <summary>Represents a piecewise function</summary>
    public class PiecewiseFunction
    {
        private readonly Interval[] _intervals;
        private readonly double[] _values;

        /// <summary>Builds a piecewise function</summary>
        /// <param name="cornerPoints">the corner points</param>
        /// <param name="values">the bucket values</param>
        public PiecewiseFunction(IList<double> cornerPoints, IList<double> values)
        {
            if (values.Count + 1 != cornerPoints.Count) throw new ArgumentException("the values' size should exactly the corner points' size + 1");

            _intervals = new Interval[values.Count];
            _values = new double[values.Count];

            for (int i = 0; i < values.Count; i++)
            {
                _intervals[0] = new Interval(cornerPoints[i], cornerPoints[i + 1], i == 0, true);
                _values[i] = values[i];
            }
        }

        /// <summary>Accesses the piecewise function for a given value</summary>
        /// <param name="t">the input abscissa</param>
        /// <returns>a double</returns>
        public double this[double t]
        {
            get
            {
                if (t < _intervals[0].LowerBound.Value || t > _intervals.Last().UpperBound.Value)
                    throw new ArgumentOutOfRangeException(nameof(t));

                int i = 0;
                while (i < _intervals.Length && !_intervals[i].Contains(t))
                    i++;
                return _values[i];
            }
        }
    }
}
