using Euclid.DataStructures.IndexedSeries;
using Euclid.Extensions;
using System;

namespace Euclid.Analytics.Regressions
{
    /// <summary>Performs a RIDGE regression for a given regularization factor</summary>
    /// <typeparam name="T">the legends</typeparam>
    /// <typeparam name="TV">the labels</typeparam>
    public class RIDGERegression<T, TV> where T : IEquatable<T>, IComparable<T> where TV : IEquatable<TV>, IConvertible
    {
        #region Declarations
        private bool _returnAverageIfFailed;
        private bool _withConstant;
        private bool _computeErr;
        private double _regularization;
        private RegressionStatus _status;
        private LinearModel _linearModel = null;
        private readonly DataFrame<T, double, TV> _x;
        private readonly Series<T, double, TV> _y;
        #endregion

        /// <summary>Builds a RIDGE to regress a <c>Series</c> on a <c>DataFrame</c></summary>
        /// <param name="x">the <c>DataFrame</c></param>
        /// <param name="y">the <c>Series</c></param>
        /// <param name="regularization">the regularization factor</param>
        public RIDGERegression(DataFrame<T, double, TV> x, Series<T, double, TV> y, double regularization)
        {
            if (x == null) throw new ArgumentNullException(nameof(x));
            if (y == null) throw new ArgumentNullException(nameof(y));
            if (x.Columns == 0 || x.Rows != y.Rows) throw new ArgumentException("the data is not consistent");
            if (regularization <= 0) throw new ArgumentOutOfRangeException(nameof(regularization), "the regularization factor should be positive");

            _x = x.Clone<DataFrame<T, double, TV>>();
            _y = y.Clone<Series<T, double, TV>>();
            _returnAverageIfFailed = false;
            _withConstant = true;
            _computeErr = true;
            _regularization = regularization;
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

        /// <summary>Gets and sets the regularization factor</summary>
        public double Regularization
        {
            get { return _regularization; }
            set
            {
                if (value <= 0) throw new ArgumentException("the regularization factor should be positive");
                _regularization = value;
            }
        }
        #endregion

        #region Get
        /// <summary>Gets the result <c>LinearModel</c></summary>
        public LinearModel LinearModel=> _linearModel; 

        /// <summary>Gets the regression's final status </summary>
        public RegressionStatus Status=> _status; 
        #endregion

        #endregion

        /// <summary>Performs the regression</summary>
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
                    X[i, 0] = 1;
                    for (int j = 0; j < p; j++) X[i, j + 1] = _x[i, j];
                }
                else
                    for (int j = 0; j < p; j++) X[i, j] = _x[i, j];
            }
            #endregion

            double yb = Y.Sum / n,
                sst = _computeErr ? (Y - yb).SumOfSquares : 0;

            #region Perform calculations
            Matrix tX = X.FastTranspose,
                tXX = Matrix.FastTransposeBySelf(X),
                intm = Matrix.LinearCombination(1, tXX, _regularization * tXX.Trace / tXX.Rows, Matrix.CreateIdentityMatrix(tXX.Rows, tXX.Columns)).FastInverse;
            if (intm == null)
            {
                if (_returnAverageIfFailed && !_withConstant)
                {
                    _linearModel = LinearModel.Create(yb, n, sst);
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
            Vector residual = null;

            if (_computeErr)
            {
                #region Error
                residual = Y - (X * A);
                sse = residual.SumOfSquares;
                #endregion

                #region Correlations
                Vector cov = tX * Y;
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
            double beta0 = _withConstant ? A[0] : 0;
            double[] beta = _withConstant ? A.Data.SubArray(1, A.Size - 1) : A.Data;
            #endregion

            _linearModel = LinearModel.Create(beta0, beta, correls, n, sse, sst - sse, residual);
            _status = RegressionStatus.Normal;
        }
    }
}
