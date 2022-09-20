using Euclid.Histograms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Euclid.Numerics
{
    public class PiecewiseFunction
    {
        private Interval[] _intervals;
        private double[] _values;

        public PiecewiseFunction(double lowerPoint, double upperPoint, IList<double> cornerPoints, IList<double> values)
        {
            if (values.Count != cornerPoints.Count + 1) throw new ArgumentException("the values' size should exactly the corner points' size + 1");

            _intervals = new Interval[cornerPoints.Count + 1];
            _values = new double[cornerPoints.Count + 1];
            for (int i = 0; i < cornerPoints.Count; i++)
            {
                if (i == 0)
                {
                    _intervals[0] = new Interval(lowerPoint, cornerPoints[0], true, true);
                    _values[0] = values[0];
                }
                else
                {
                    _intervals[i] = new Interval(cornerPoints[i - 1], cornerPoints[i], false, true);
                    _values[i] = values[i];
                }
            }
            _intervals[cornerPoints.Count] = new Interval(cornerPoints.Last(), upperPoint, false, true);
            _values[cornerPoints.Count] = values.Last();
        }

        public double this[double t]
        {
            get
            {
                int i = 0;
                while (i < _intervals.Length && !_intervals[i].Contains(t))
                    i++;
                return _values[i];
            }
        }
    }
}
