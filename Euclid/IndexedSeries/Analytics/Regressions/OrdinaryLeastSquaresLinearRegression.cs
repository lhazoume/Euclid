﻿using System;
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

            #region Load data
            int n = _y.Rows, p = _x.Columns;
            Matrix X = new Matrix(n, p + (_withConstant ? 1 : 0)), Y = new Matrix(n, 1);
            for (int i = 0; i < n; i++)
            {
                Y[i] = _y[i];
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

            Matrix radix = intm ^ tX,
                A = radix ^ Y;
            #endregion

            double sse = 0;
            double[] correls = new double[p];
            if (_computeErr)
            {
                Matrix H = X ^ radix,
                    I = Matrix.IdentityMatrix(H.Rows, H.Columns),
                    e = Y.FastTranspose ^ (I - H) ^ Y,
                    cov = tX ^ Y;

                #region Correlations
                for (int i = 0; i < p; i++)
                {
                    double xb = X.Column(1 + i).Sum / n,
                        sX = tXX[1 + i, 1 + i] / n - xb * xb,
                        cXY = cov[1 + i] / n - yb * xb;
                    correls[i] = cXY / Math.Sqrt(sX * sst);
                }
                #endregion

                sse = e[0];
            }
            #endregion

            #region Output
            List<double> beta = new List<double>();
            double beta0 = _withConstant ? A[0] : 0;
            for (int i = (_withConstant ? 1 : 0); i < A.Rows; i++) beta.Add(A[i * A.Columns]);
            #endregion

            _linearModel = new LinearModel(beta0, beta.ToArray(), correls, n, sse, sst - sse);
            _status = RegressionStatus.Normal;
        }
    }
}
