using Euclid.IndexedSeries;
using System;
using System.Collections.Generic;

namespace Euclid.Analytics.Regressions
{
    /// <summary>OrdinaryLeastSquaresLinearRegression class</summary>
    /// <typeparam name="T">the legends' type</typeparam>
    /// <typeparam name="V">the labels' type</typeparam>
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

        /// <summary>Builds a OLS to regress a <c>Series</c> on a <c>DataFrame</c></summary>
        /// <param name="x">the <c>DataFrame</c></param>
        /// <param name="y">the <c>Series</c></param>
        public OrdinaryLeastSquaresLinearRegression(DataFrame<T, double, V> x, Series<T, double, V> y)
        {
            if (x == null || y == null) throw new ArgumentNullException("the x and y should not be null");
            if (x.Columns == 0 || x.Rows != y.Rows) throw new ArgumentException("the data is not consistent");

            _x = x.Clone();
            _y = y.Clone();
            _returnAverageIfFailed = false;
            _withConstant = true;
            _computeErr = true;
            _status = RegressionStatus.NotRan;
        }

        #region  Accessors

        #region Settables
        /// <summary>Gets and sets whether the Y's average should be return when the regression fails</summary>
        public bool ReturnAverageIfFailed
        {
            get { return _returnAverageIfFailed; }
            set { _returnAverageIfFailed = value; }
        }

        /// <summary>Gets and sets whether the regression should involve a constant term</summary>
        public bool WithConstant
        {
            get { return _withConstant; }
            set { _withConstant = value; }
        }

        /// <summary>Gets and sets whether the errors should be computed after the regression</summary>
        public bool ComputeError
        {
            get { return _computeErr; }
            set { _computeErr = value; }
        }
        #endregion

        #region Get
        /// <summary>
        /// Gets the result <c>LinearModel</c>
        /// </summary>
        public LinearModel LinearModel
        {
            get { return _linearModel; }
        }

        /// <summary>
        /// Gets the regression's final status
        /// </summary>
        public RegressionStatus Status
        {
            get { return _status; }
        }
        #endregion

        #endregion

        /// <summary>
        /// Performs the regression
        /// </summary>
        public void Regress()
        {
            #region Matrices

            #region Load data
            int n = _y.Rows, p = _x.Columns;
            Matrix X = Matrix.Create(n, p + (_withConstant ? 1 : 0));
            Vector Y = Vector.Create(_y.Data);
            for (int i = 0; i < n; i++)
            {
                if (_withConstant)
                {
                    X[i * (p + 1)] = 1;
                    for (int j = 0; j < p; j++) X[i * (p + 1) + j + 1] = _x[i, j];
                }
                else
                    for (int j = 0; j < p; j++) X[i * p + j] = _x[i, j];
            }
            #endregion

            double yb = Y.Sum / n,
                sst = _computeErr ? Y.SumOfSquares - n * yb * yb : 0;

            #region Perform calculations
            Matrix tX = X.FastTranspose,
                tXX = Matrix.FastTransposeBySelf(X),
                intm = tXX.FastInverse;
            if (intm == null)
            {
                if (_returnAverageIfFailed && !_withConstant)
                {
                    _linearModel = new LinearModel(yb, n, sst);
                    _status = RegressionStatus.Normal;
                }
                else
                    _status = RegressionStatus.BadData;
                return;
            }

            Matrix radix = intm ^ tX;
            Vector A = radix * Y;
            #endregion

            double sse = 0;
            double[] correls = new double[p];
            if (_computeErr)
            {
                Matrix H = X ^ radix,
                    I = Matrix.CreateIdentityMatrix(H.Rows, H.Columns);
                sse = Vector.Scalar(Y, (I - H) * Y);
                Vector cov = tX * Y;

                #region Correlations
                for (int i = 0; i < p; i++)
                {
                    double xb = X.Column(1 + i).Sum / n,
                        sX = tXX[1 + i, 1 + i] / n - xb * xb,
                        cXY = cov[1 + i] / n - yb * xb;
                    correls[i] = cXY / Math.Sqrt(sX * sst);
                }
                #endregion
            }
            #endregion

            #region Output
            List<double> beta = new List<double>();
            double beta0 = _withConstant ? A[0] : 0;
            for (int i = (_withConstant ? 1 : 0); i < A.Size; i++) beta.Add(A[i]);
            #endregion

            _linearModel = new LinearModel(beta0, beta.ToArray(), correls, n, sse, sst - sse);
            _status = RegressionStatus.Normal;
        }
    }
}
