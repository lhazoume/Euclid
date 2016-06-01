using System;
using System.Collections.Generic;
using System.Linq;

namespace Euclid.IndexedSeries.Analytics.Regressions
{
    public sealed class Scaling
    {
        #region Private Variables
        private double _intercept, _scalingCoefficient;
        #endregion

        #region Constructor
        private Scaling(double intercept, double scalingCoefficient)
        {
            _intercept = intercept;
            _scalingCoefficient = scalingCoefficient;
        }
        # endregion

        #region Accessors
        public double Intercept
        {
            get { return _intercept; }
        }
        public double ScalingCoefficient
        {
            get { return _scalingCoefficient; }
        }
        #endregion

        #region Methods

        public double[] Scale(IEnumerable<double> data)
        {
            double[] result = new double[data.Count()];

            for (int i = 0; i < result.Length; i++)
                result[i] = _intercept + _scalingCoefficient * data.ElementAt(i);

            return result;
        }

        public double[] Reduce(IEnumerable<double> data)
        {
            double[] result = new double[data.Count()];

            for (int i = 0; i < result.Length; i++)
                result[i] = (data.ElementAt(i) - _intercept) / _scalingCoefficient;

            return result;
        }

        #endregion

        #region Creators

        public static Scaling CreateZScore(IEnumerable<double> data)
        {
            int n = data.Count();
            double intercept = 0, scaling = 0;

            foreach (double element in data)
            {
                intercept += element;
                scaling += element * element;
            }

            intercept /= n;
            double sd = Math.Sqrt(scaling / n - intercept * intercept);

            if (sd == 0) return null;

            Scaling result = new Scaling(intercept, sd);
            return result;
        }

        public static Scaling CreateMinMax(IEnumerable<double> data)
        {
            int n = data.Count();
            double min = data.First(), max = min;

            foreach (double element in data)
            {
                if (element > max)
                    max = element;
                else if (element < min)
                    min = element;
            }

            double sd = Math.Sqrt(max - min);

            if (sd == 0) return null;

            Scaling result = new Scaling(min, sd);
            return result;
        }
        
        #endregion
    }
}
