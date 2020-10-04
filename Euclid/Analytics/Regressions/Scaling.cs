using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Euclid.Analytics.Regressions
{
    /// <summary>Scaling class</summary>
    public sealed class Scaling
    {
        #region Private Variables
        private readonly double _intercept, _scalingCoefficient;
        #endregion

        #region Constructor
        private Scaling(double intercept, double scalingCoefficient)
        {
            _intercept = intercept;
            _scalingCoefficient = scalingCoefficient;
        }
        # endregion

        #region Accessors
        /// <summary>Gets the intercept of the Scaling</summary>
        public double Intercept => _intercept;

        /// <summary>Gets the scaling coefficient of the Scaling</summary>
        public double ScalingCoefficient => _scalingCoefficient;
        #endregion

        #region Methods

        /// <summary>Scales "up" a series of data</summary>
        /// <param name="data">the data to scale "up" : intercept + x * scalingCoefficient</param>
        /// <returns>an array of double</returns>
        public double[] Scale(IEnumerable<double> data)
        {
            double[] result = new double[data.Count()];

            for (int i = 0; i < result.Length; i++)
                result[i] = _intercept + _scalingCoefficient * data.ElementAt(i);

            return result;
        }

        /// <summary>Scales "down" a series of data</summary>
        /// <param name="data">the data to scale "down" (x-intercept)/scalingCoefficient</param>
        /// <returns>an array of double</returns>
        public double[] Reduce(IEnumerable<double> data)
        {
            double[] result = new double[data.Count()];

            for (int i = 0; i < result.Length; i++)
                result[i] = (data.ElementAt(i) - _intercept) / _scalingCoefficient;

            return result;
        }

        #endregion

        #region Creators

        /// <summary>Creates a Scaling based on the average and the standard deviation</summary>
        /// <param name="data">the data to scale</param>
        /// <returns>a <c>Scaling</c> class</returns>
        public static Scaling CreateZScore(IEnumerable<double> data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

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

        /// <summary>Creates a Scaling based on the minimum and maximum</summary>
        /// <param name="data">the data to scale</param>
        /// <returns>a <c>Scaling</c> class</returns>
        public static Scaling CreateMinMax(IEnumerable<double> data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
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
