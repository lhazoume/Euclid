using System;
using System.Collections.Generic;

namespace Euclid.IndexedSeries.Analytics.Regressions
{
    public class OrdinaryLeastSquaresLinearRegression<T, V> where T : IEquatable<T>, IComparable<T> where V : IEquatable<V>, IConvertible
    {
        #region Declarations
        private bool _returnAverageIfFailed;
        private bool _withConstant;
        private bool _computeErr;
        private RegressionStatus _status;
        private LinearModel _linearModel = null;
        private DataFrame<T, double, V> _x;
        private Series<T, double, V> _y;
        #endregion

        public OrdinaryLeastSquaresLinearRegression(DataFrame<T, double, V> x, Series<T, double, V> y)
        {
            if (x == null || y == null) throw new ArgumentNullException("the x and y should not be null");
            if (_x.Columns == 0 || _x.Rows != _y.Rows) throw new ArgumentException("the data is not consistent");

            _x = x.Clone();
            _y = y.Clone();
            _returnAverageIfFailed = false;
            _withConstant = true;
            _computeErr = true;
            _status = RegressionStatus.NotRan;
        }

        #region  Accessors

        #region Settables
        public bool ReturnAverageIfFailed
        {
            get { return _returnAverageIfFailed; }
            set { _returnAverageIfFailed = value; }
        }
        public bool WithConstant
        {
            get { return _withConstant; }
            set { _withConstant = value; }
        }
        public bool ComputeError
        {
            get { return _computeErr; }
            set { _computeErr = value; }
        }
        #endregion

        #region Get
        public LinearModel LinearModel
        {
            get { return _linearModel; }
        }
        public RegressionStatus Status
        {
            get { return _status; }
        }
        #endregion

        #endregion

        public void Regress()
        {
            #region Matrices
            int n = _y.Rows, p = _x.Columns;
            Matrix X = new Matrix(n, p + (_withConstant ? 1 : 0)), Y = new Matrix(n, 1);
            for (int i = 0; i < n; i++)
            {
                Y[i] = _y[i];
                if (_withConstant)
                {
                    X[i * (p + 1)] = 1;
                    for (int j = 1; j <= p; j++) X[i * (p + 1) + j] = _x[i, j - 1];
                }
                else
                    for (int j = 0; j < p; j++) X[i * p + j] = _x[i, j];
            }
            double yb = Y.Sum / n,
                sst = _computeErr ? Y.SumOfSquares - n * yb * yb : 0;

            Matrix tX = X.FastTranspose,
                intm = Matrix.FastTransposeBySelf(X).FastInverse;
            if (intm == null)
            {
                if (_returnAverageIfFailed && !_withConstant)
                {
                    _linearModel = new LinearModel(yb, n, sst);
                    _status = RegressionStatus.Normal;
                }
                else
                    _status = RegressionStatus.BadData;
            }

            Matrix radix = intm ^ tX,
                A = radix ^ Y;

            double sse = 0;
            if (_computeErr)
            {
                Matrix H = X ^ radix,
                    e = Y.FastTranspose ^ (Matrix.IdentityMatrix(H.Rows, H.Columns) - H) ^ Y;
                sse = e[0];
            }
            #endregion

            #region Output
            List<double> beta = new List<double>();
            double beta0 = _withConstant ? A[0] : 0;
            for (int i = (_withConstant ? 1 : 0); i < A.Rows; i++) beta.Add(A[i * A.Columns]);
            #endregion

            _linearModel = new LinearModel(beta0, beta.ToArray(), n, sse, sst - sse);
            _status = RegressionStatus.Normal;
        }
    }
}
