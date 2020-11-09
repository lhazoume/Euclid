using System;
using System.Linq;
using System.Threading.Tasks;

namespace Euclid.Numerics.PartialDifferentialEquations
{
    /// <summary>Represents a bivariate partial derivative equation</summary>
    public class BivariatePDE
    {
        #region Variables
        private readonly BivariateGridConfiguration _config;
        private readonly Matrix[] _slices;
        #endregion

        /// <summary>Builds a Bivariate Partial Derivative Equation grid</summary>
        /// <param name="configuration">the grid configuration</param>
        public BivariatePDE(BivariateGridConfiguration configuration)
        {
            _config = configuration;
            _slices = Enumerable.Range(0, _config.TimeCount).Select(i => Matrix.Create(_config.XCount, _config.YCount, 0.0)).ToArray();
        }

        #region Methods

        #region Derivatives
        /// <summary>Returns the grid value for a given date, x index, and y index</summary>
        /// <param name="t">the date index</param>
        /// <param name="xIndex">the x index</param>
        /// <param name="yIndex">the y index</param>
        /// <returns>a double</returns>
        public double Value(int t, int xIndex, int yIndex)
        {
            return _slices[t][xIndex, yIndex];
        }

        /// <summary>Index of the value matrix</summary>
        public const int VALUE = 0;
        /// <summary>Index of the x derivative matrix</summary>
        public const int XDERIVATIVE = 1;
        /// <summary>Index of the y derivative matrix</summary>
        public const int YDERIVATIVE = 2;
        /// <summary>Index of the x second order derivative matrix</summary>
        public const int XCONVEXITY = 3;
        /// <summary>Index of the y second order derivative matrix</summary>
        public const int YCONVEXITY = 4;
        /// <summary>Index of the xy cross derivative matrix</summary>
        public const int XYCONVECITY = 5;

        /// <summary>Returns the value and spatial first and second order derivatives for a given date</summary>
        /// <param name="t">the date index</param>
        /// <returns>a set of <c>Matrix</c></returns>
        public Matrix[] Matrices(int t)
        {

            int n = _config.XCount * _config.YCount;
            Matrix[] result = Enumerable.Range(0, 6).Select(i => Matrix.Create(_config.XCount, _config.YCount, 0.0)).ToArray();
            Parallel.For(0, n, k =>
            {
                int i = k / _config.YCount,
                    j = k % _config.YCount,
                    im = Math.Max(i - 1, 0), ip = Math.Min(i + 1, _config.XCount - 1),
                    jm = Math.Max(j - 1, 0), jp = Math.Min(j + 1, _config.YCount - 1);

                result[VALUE][i, j] = _slices[t][i, j];
                result[XDERIVATIVE][i, j] = (_slices[t][ip, j] - _slices[t][im, j]) / (_config.XBuckets[ip] - _config.XBuckets[im]);
                result[YDERIVATIVE][i, j] = (_slices[t][i, jp] - _slices[t][i, jm]) / (_config.YBuckets[jp] - _config.YBuckets[jm]);

                #region XCONVEXITY
                if (i == 0)
                    result[XCONVEXITY][0, j] = 2 / (_config.XIncrements[0] * (_config.XIncrements[1] + _config.XIncrements[0])) * (_slices[t][0, j] + (_config.XIncrements[0] * _slices[t][1, j] - (_config.XIncrements[1] + _config.XIncrements[0]) * _slices[t][2, j]) / _config.XIncrements[1]);
                else if (i < _config.XCount - 1)
                    result[XCONVEXITY][i, j] = -2 / (_config.XIncrements[i] * _config.XIncrements[i - 1]) * (_slices[t][i, j] - (_config.XIncrements[i] * _slices[t][i - 1, j] + _config.XIncrements[i - 1] * _slices[t][i + 1, j]) / (_config.XIncrements[i] + _config.XIncrements[i - 1]));
                else
                    result[XCONVEXITY][_config.XCount - 1, j] = 2 / (_config.XIncrements[_config.XCount - 2] * (_config.XIncrements[_config.XCount - 3] + _config.XIncrements[_config.XCount - 2])) * (_slices[t][_config.XCount - 1, j] - (-_config.XIncrements[_config.XCount - 2] * _slices[t][_config.XCount - 3, j] + (_config.XIncrements[_config.XCount - 3] + _config.XIncrements[_config.XCount - 2]) * _slices[t][_config.XCount - 2, j]) / _config.XIncrements[_config.XCount - 3]);
                #endregion

                #region YCONVEXITY
                if (j == 0)
                    result[YCONVEXITY][i, 0] = 2 / (_config.YIncrements[0] * (_config.YIncrements[1] + _config.YIncrements[0])) * (_slices[t][i, 0] + (_config.YIncrements[0] * _slices[t][i, 1] - (_config.YIncrements[1] + _config.YIncrements[0]) * _slices[t][i, 2]) / _config.YIncrements[1]);
                else if (j < _config.YCount - 1)
                    result[YCONVEXITY][i, j] = -2 / (_config.YIncrements[j] * _config.YIncrements[j - 1]) * (_slices[t][i, j] - (_config.YIncrements[j] * _slices[t][i, j - 1] + _config.YIncrements[j - 1] * _slices[t][i, j + 1]) / (_config.YIncrements[j] + _config.YIncrements[j - 1]));
                else
                    result[YCONVEXITY][i, _config.YCount - 1] = 2 / (_config.YIncrements[_config.YCount - 2] * (_config.YIncrements[_config.YCount - 3] + _config.YIncrements[_config.YCount - 2])) * (_slices[t][i, _config.YCount - 1] - (-_config.YIncrements[_config.YCount - 2] * _slices[t][i, _config.YCount - 3] + (_config.YIncrements[_config.YCount - 3] + _config.YIncrements[_config.YCount - 2]) * _slices[t][i, _config.YCount - 2]) / _config.YIncrements[_config.YCount - 3]);
                #endregion

                #region XYCONVEXITY
                if (i == 0)
                {
                    if (j == 0)
                        result[XYCONVECITY][0, 0] = (_slices[t][1, 1] - _slices[t][0, 1] + _slices[t][0, 0] - _slices[t][1, 0]) / (_config.YIncrements[0] * _config.XIncrements[0]);
                    else if (j < _config.YCount - 1)
                        result[XYCONVECITY][0, j] = (_slices[t][1, j + 1] - _slices[t][0, j + 1] + _slices[t][0, j - 1] - _slices[t][1, j - 1]) / ((_config.YBuckets[j + 1] - _config.YBuckets[j - 1]) * _config.XIncrements[0]);
                    else
                        result[XYCONVECITY][0, _config.YCount - 2] = -(_slices[t][1, _config.YCount - 1] - _slices[t][0, _config.YCount - 1] + _slices[t][0, _config.YCount - 2] - _slices[t][1, _config.YCount - 2]) / (_config.YIncrements[_config.YCount - 2] * _config.XIncrements[0]);
                }
                else if (i < _config.XCount - 1)
                {
                    if (j == 0)
                        result[XYCONVECITY][i, 0] = (_slices[t][i + 1, 1] - _slices[t][i - 1, 1] + _slices[t][i - 1, 0] - _slices[t][i + 1, 0]) / (_config.YIncrements[0] * (_config.XIncrements[i] + _config.XIncrements[i - 1]));
                    else if (j < _config.YCount - 1)
                        result[XYCONVECITY][i, j] = (_slices[t][i + 1, j + 1] - _slices[t][i - 1, j + 1] + _slices[t][i - 1, j - 1] - _slices[t][i + 1, j - 1]) / ((_config.YBuckets[j + 1] - _config.YBuckets[j - 1]) * (_config.XBuckets[i + 1] - _config.XBuckets[i - 1]));
                    else
                        result[XYCONVECITY][i, _config.YCount - 1] = -(_slices[t][i + 1, _config.YCount - 1] - _slices[t][i - 1, _config.YCount - 1] + _slices[t][i - 1, _config.YCount - 2] - _slices[t][i + 1, _config.YCount - 2]) / (_config.YIncrements[_config.YCount - 2] * (_config.XBuckets[i + 1] - _config.XBuckets[i - 1]));
                }
                else
                {
                    if (j == 0)
                        result[XYCONVECITY][_config.XCount - 1, 0] = -(_slices[t][_config.XCount - 1, 1] - _slices[t][_config.XCount - 2, 1] + _slices[t][_config.XCount - 2, 0] - _slices[t][_config.XCount - 1, 0]) / (_config.YIncrements[0] * _config.XIncrements[_config.XCount - 2]);
                    else if (j < _config.YCount - 1)
                        result[XYCONVECITY][_config.XCount - 1, j] = -(_slices[t][_config.XCount - 1, j + 1] - _slices[t][_config.XCount - 2, j + 1] + _slices[t][_config.XCount - 2, j - 1] - _slices[t][_config.XCount - 1, j - 1]) / ((_config.YBuckets[j + 1] - _config.YBuckets[j - 1]) * _config.XIncrements[_config.XCount - 2]);
                    else
                        result[XYCONVECITY][_config.XCount - 1, _config.YCount - 1] = (_slices[t][_config.XCount - 1, _config.YCount - 1] - _slices[t][_config.XCount - 2, _config.YCount - 1] + _slices[t][_config.XCount - 2, _config.YCount - 2] - _slices[t][_config.XCount - 1, _config.YCount - 2]) / (_config.YIncrements[_config.YCount - 2] * _config.XIncrements[_config.XCount - 2]);
                }

                #endregion

            });

            return result;
        }
        #endregion

        /// <summary>Sets the grid value for a given date</summary>
        /// <param name="t">the date index</param>
        /// <param name="value">the value grid</param>
        public void Set(int t, Matrix value)
        {
            _slices[t] = value;
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
