using System;
using System.Linq;
using System.Threading.Tasks;

namespace Euclid.Numerics.PartialDifferentialEquations
{
    public class UnivariatePDE
    {
        #region Variables
        private readonly UnivariateGridConfiguration _config;
        private readonly Vector[] _slices;
        #endregion

        public UnivariatePDE(UnivariateGridConfiguration configuration)
        {
            _config = configuration;
            _slices = Enumerable.Range(0, _config.TimeCount).Select(i => Vector.Create(_config.XCount, 0.0)).ToArray();
        }

        #region Methods

        #region Derivatives
        /// <summary>Returns the grid value for a given date, x index</summary>
        /// <param name="t">the date index</param>
        /// <param name="xIndex">the x index</param>
        /// <returns>a double</returns>
        public double Value(int t, int xIndex)
        {
            return _slices[t][xIndex];
        }


        /// <summary>Returns the value and spatial first and second order derivatives for a given date</summary>
        /// <returns>a set of <c>Vector</c></returns>
        public Vector Retropropagate(double upP, double midP, double dnP, Vector value)
        {
            Vector result = Vector.Create(_config.XCount, 0.0);

            for (int i = 1; i < _config.XCount - 1; i++)
                result[i] = midP * value[i] + upP * value[i + 1] + dnP * value[i - 1];

            return result;
        }
        #endregion

        /// <summary>Sets the grid value for a given date</summary>
        /// <param name="t">the date index</param>
        /// <param name="value">the value grid</param>
        public Vector this[int t]
        {
            get { return _slices[t]; }
            set { _slices[t] = value; }
        }

        /// <summary>Computes the exercice frontier</summary>
        /// <param name="expected">the expected value matrix</param>
        /// <param name="intrinsic">the intrinsic value matrix</param>
        /// <returns>a <c>Matrix</c></returns>
        public static Matrix ExerciceFrontier(Matrix expected, Matrix intrinsic)
        {
            if (expected.Rows != intrinsic.Rows || expected.Columns != intrinsic.Columns)
                throw new ArgumentException("the expected and intrinsic matrices do not match");

            Matrix result = Matrix.Create(expected.Rows, expected.Columns, 0.0);
            Parallel.For(0, expected.Size, k =>
            {
                int i = k / expected.Columns,
                    j = k % expected.Columns;
                result[i, j] = intrinsic[i, j] > expected[i, j] ? 1 : 0;
            });
            return result;
        }
        #endregion
    }
}
