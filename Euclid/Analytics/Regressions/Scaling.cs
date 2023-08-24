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
        /// <param name="centering">Enable centering data (x-intercept) part</param>
        /// <param name="scaling">Enable scaling data /scalingCoefficient part</param>
        /// <returns>an array of double</returns>
        public double[] Reduce(IReadOnlyList<double> data, bool centering = true, bool scaling = true)
        {
            #region initialization
            double intercept = centering ? _intercept : 0, scaleCoeffcient = scaling ? _scalingCoefficient : 1;
            double[] result = new double[data.Count()];
            #endregion

            for (int i = 0; i < result.Length; i++) result[i] = (data[i] - intercept) / scaleCoeffcient;

            return result;
        }

        /// <summary>Scales "down" a series of data</summary>
        /// <param name="data">the data to scale "down" (x-intercept)/scalingCoefficient</param>
        /// <param name="centering">Enable centering data (x-intercept) part</param>
        /// <param name="scaling">Enable scaling data /scalingCoefficient part</param>
        /// <returns>an array of double</returns>
        public Matrix Reduce(Matrix data, bool centering = true, bool scaling = true)
        {
            #region initialization
            double intercept = centering ? _intercept : 0, scaleCoeffcient = scaling ? _scalingCoefficient : 1;
            Matrix result = Matrix.Create(data.Rows, data.Columns);
            #endregion

            for (int i = 0; i < result.Rows; i++) 
                for(int j = 0; j < result.Columns; j++)
                    result[i,j] = (data[i, j] - intercept) / scaleCoeffcient;

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

        /// <summary>Creates a Scaling based on the average and the standard deviation</summary>
        /// <param name="data">the data to scale</param>
        /// <returns>a <c>Scaling</c> class</returns>
        public static Scaling CreateZScore(Matrix data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            int n = data.Rows * data.Columns;
            double intercept = 0, scaling = 0;

            for(int i = 0; i < n; i++)
                for(int j = 0; j < n; j++)
                {
                    intercept += data[i,j];
                    scaling += data[i, j] * data[i, j];
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
