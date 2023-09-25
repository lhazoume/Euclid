using Euclid.DataStructures.IndexedSeries;
using System;
using System.Collections.Generic;

namespace Euclid.Analytics.Regressions
{
    /// <summary>OrdinaryLeastSquaresLinearRegression class</summary>
    public class OrdinaryLeastSquaresLinearRegression
    {
        #region Declarations
        private bool _returnAverageIfFailed;
        private bool _withConstant;
        private bool _computeErr;
        private RegressionStatus _status;
        private LinearModel _linearModel = null;
        private readonly double[][] _x;
        private readonly double[] _y;
        #endregion

        #region constructor
        /// <summary>Builds an OLS to regress a on a set of predictors</summary>
        /// <param name="x">the <c>Predictor(s)</c></param>
        /// <param name="y">the <c>Regressor</c></param>
        private OrdinaryLeastSquaresLinearRegression(double[][] x, double[] y)
        {
            if (x == null) throw new ArgumentNullException(nameof(x));
            if (y == null) throw new ArgumentNullException(nameof(y));
            if (x[0].Length == 0 || x.Length != y.Length) throw new ArgumentException("the data is not consistent");

            _x = x;
            _y = y;
            _returnAverageIfFailed = false;
            _withConstant = true;
            _computeErr = true;
            _status = RegressionStatus.NotRan;
        }
        #endregion

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
        /// <summary>Gets the result <c>LinearModel</c></summary>
        public LinearModel LinearModel => _linearModel;

        /// <summary>Gets the regression's final status</summary>
        public RegressionStatus Status => _status;
        #endregion

        #endregion

        #region methods

        #region creation
        /// <summary>
        /// Create an OLS object in order to regress a on a set of predictors
        /// </summary>
        /// <param name="x">the <c>Predictor(s)</c></param>
        /// <param name="y">the <c>Regressor</c></param>
        /// <returns>OLS object</returns>
        public static OrdinaryLeastSquaresLinearRegression Create(double[][] x, double[] y) { return new OrdinaryLeastSquaresLinearRegression(x, y); }

        /// <summary>
        /// Create an OLS object in order to regress a on a set of predictors
        /// </summary>
        /// <typeparam name="T">Legends</typeparam>
        /// <typeparam name="TV">Labels</typeparam>
        /// <param name="x">the <c>Predictor(s)</c></param>
        /// <param name="y">the <c>Regressor</c></param>
        /// <param name="deepCopy">Force deep copy</param>
        /// <returns>OLS object</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static OrdinaryLeastSquaresLinearRegression Create<T, TV>(DataFrame<T, double, TV> x, Series<T, double, TV> y, bool deepCopy = false) where T : IEquatable<T>, IComparable<T> where TV : IEquatable<TV>, IConvertible
        {
            #region requirements
            if (x == null) throw new ArgumentNullException(nameof(x));
            if (y == null) throw new ArgumentNullException(nameof(y));
            if (x.Columns == 0 || x.Rows != y.Rows) throw new ArgumentException("the data is not consistent");
            #endregion

            if(deepCopy) return new OrdinaryLeastSquaresLinearRegression(x.Data, y.Data);

            DataFrame<T, double, TV> x_ = x.Clone<DataFrame<T, double, TV>>();
            Series<T, double, TV> y_ = y.Clone<Series<T, double, TV>>();

            return new OrdinaryLeastSquaresLinearRegression(x_.Data, y_.Data);
        }
        #endregion

        /// <summary>
        /// Performs the regression
        /// </summary>
        public void Regress()
        {
            #region Matrices

            #region Load data
            int n = _y.Length, p = _x[0].Length;
            Matrix X = Matrix.Create(n, p + (_withConstant ? 1 : 0));
            Vector Y = Vector.Create(_y);
            for (int i = 0; i < n; i++)
            {
                if (_withConstant)
                {
                    X[i, 0] = 1;
                    for (int j = 0; j < p; j++) X[i, j + 1] = _x[i][j];
                    /*X[i * (p + 1)] = 1;
                    for (int j = 0; j < p; j++) X[i * (p + 1) + j + 1] = _x[i, j];*/
                }
                else
                    for (int j = 0; j < p; j++) X[i, j] = _x[i][j];
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
            //if (_computeErr)
            //{
            //    Matrix H = X ^ radix,
            //        I = Matrix.CreateIdentityMatrix(H.Rows, H.Columns);
            //    sse = Vector.Scalar(Y, (I - H) * Y);
            //    Vector cov = tX * Y;

            //    #region Correlations
            //    for (int i = 0; i < p; i++)
            //    {
            //        double xb = X.Column((_withConstant ? 1 : 0) + i).Sum / n,
            //            sX = tXX[(_withConstant ? 1 : 0) + i, (_withConstant ? 1 : 0) + i] / n - xb * xb,
            //            cXY = cov[(_withConstant ? 1 : 0) + i] / n - yb * xb;
            //        correls[i] = cXY / Math.Sqrt(sX * sst);
            //    }
            //    #endregion
            //}

            if (_computeErr)
            {
                #region Error
                Vector residual = Y - (X * A);
                sse = residual.SumOfSquares;
                #endregion

                #region Correlations
                Vector cov = tX * Y;
                for (int i = 0; i < p; i++)
                {
                    double xb = X.Column((_withConstant ? 1 : 0) + i).Sum / n,
                        sX = tXX[(_withConstant ? 1 : 0) + i, (_withConstant ? 1 : 0) + i] / n - xb * xb,
                        cXY = cov[(_withConstant ? 1 : 0) + i] / n - yb * xb;
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
        #endregion
    }
}
