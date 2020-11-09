using System;
using System.Linq;

namespace Euclid.Numerics.PartialDifferentialEquations
{
    public class UnivariatePDE
    {
        #region Variables
        private readonly int _nbT, _nbX;
        private readonly double[] _ts, _xs;
        private readonly Vector[] _slices;

        private readonly Matrix _firstDerivative, _secondDerivative;
        private double[] _dx;
        #endregion

        public UnivariatePDE(double[] timeBuckets, double[] valueBuckets)
        {
            _ts = timeBuckets.Distinct().OrderBy(d => d).ToArray();
            _xs = valueBuckets.Distinct().OrderBy(d => d).ToArray();

            _nbT = _ts.Length;
            _nbX = _xs.Length;

            _firstDerivative = Matrix.Create(_nbX, _nbX);
            _secondDerivative = Matrix.Create(_nbX, _nbX);

            CalculateIncrements();
            BuildMatrices();

            _slices = Enumerable.Range(0, _nbT).Select(i => Vector.Create(_nbX, 0.0)).ToArray();
        }

        private void CalculateIncrements()
        {
            _dx = new double[_nbX - 1];
            for (int i = 0; i < _nbX - 1; i++)
                _dx[i] = _xs[i + 1] - _xs[i];
        }
        private void BuildMatrices()
        {
            #region First derivative
            _firstDerivative[0, 0] = -1 / _dx[0];
            _firstDerivative[0, 1] = 1 / _dx[0];

            for (int i = 1; i < _nbX - 1; i++)
            {
                _firstDerivative[i, i + 1] = 1 / (_dx[i] + _dx[i - 1]);
                _firstDerivative[i, i - 1] = -1 / (_dx[i] + _dx[i - 1]);
            }

            _firstDerivative[_nbX - 1, _nbX - 1] = 1 / _dx[_nbX - 2];
            _firstDerivative[_nbX - 1, _nbX - 2] = -1 / _dx[_nbX - 2];
            #endregion

            #region Second derivative
            double d0x2 = _dx[0] * _dx[0];
            _secondDerivative[0, 0] = 1 / d0x2;
            _secondDerivative[0, 1] = -2 / d0x2;
            _secondDerivative[0, 2] = 1 / d0x2;

            for (int i = 1; i < _nbX - 1; i++)
            {
                double dx2 = Math.Pow((_dx[i] + _dx[i - 1]) / 2, 2);
                _secondDerivative[i, i + 1] = 1 / dx2;
                _secondDerivative[i, i] = -2 / dx2;
                _secondDerivative[i, i - 1] = 1 / dx2;
            }

            double dlx2 = _dx[_nbX - 1] * _dx[_nbX - 1];
            _secondDerivative[_nbX - 1, _nbX - 1] = 1 / dlx2;
            _secondDerivative[_nbX - 1, _nbX - 2] = -2 / dlx2;
            _secondDerivative[_nbX - 1, _nbX - 3] = 1 / dlx2;
            #endregion
        }

        /// <summary>Returns the numerical x-derivative on a grid</summary>
        /// <param name="t">the time index</param>
        /// <returns>the derivative vector</returns>
        public Vector Derivative(int t)
        {
            return _firstDerivative * _slices[t];
        }

        /// <summary>Returns the numerical x-convexity on a grid</summary>
        /// <param name="t">the time index</param>
        /// <returns>the convexity vector</returns>
        public Vector Convexity(int t)
        {
            return _secondDerivative * _slices[t];
        }
    }
}
