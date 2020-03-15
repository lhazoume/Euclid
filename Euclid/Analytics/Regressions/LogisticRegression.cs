using Euclid.DataStructures.IndexedSeries;
using Euclid.Solvers;
using System;
using System.Linq;

namespace Euclid.Analytics.Regressions
{
    /// <summary>OrdinaryLeastSquaresLinearRegression class</summary>
    /// <typeparam name="T">the legends' type</typeparam>
    /// <typeparam name="V">the labels' type</typeparam>
    public class LogisticRegression<T, V> where T : IEquatable<T>, IComparable<T> where V : IEquatable<V>, IConvertible
    {
        #region Declarations
        private bool _returnAverageIfFailed, _withConstant;
        private RegressionStatus _status;
        private double _finalLogLikelihood = double.MinValue;
        private LogisticModel _logisticModel = null;
        private DataFrame<T, double, V> _x;
        private Series<T, double, V> _y;
        #endregion

        /// <summary>Builds a OLS to regress a <c>Series</c> on a <c>DataFrame</c></summary>
        /// <param name="x">the <c>DataFrame</c></param>
        /// <param name="y">the <c>Series</c></param>
        public LogisticRegression(DataFrame<T, double, V> x, Series<T, double, V> y)
        {
            if (x == null || y == null) throw new ArgumentNullException("the x and y should not be null");
            if (x.Columns == 0 || x.Rows != y.Rows) throw new ArgumentException("the data is not consistent");

            _x = x.Clone();
            _y = y.Clone();
            _returnAverageIfFailed = false;
            _withConstant = true;
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
        #endregion

        #region Get
        /// <summary>Returns the result <c>LinearModel</c></summary>
        public LogisticModel LogisticModel
        {
            get { return _logisticModel; }
        }

        /// <summary>Gets the regression's final status</summary>
        public RegressionStatus Status
        {
            get { return _status; }
        }

        /// <summary>Returns the final log-likelihood of the regression</summary>
        public double LogLikelihood
        {
            get { return _finalLogLikelihood; }
        }
        #endregion

        #endregion

        private static double Func(Vector theta, Vector[] Xrows, Vector Y)
        {
            double result = 0,
                p = 0;
            for (int i = 0; i < Y.Size; i++)
            {
                p = Fn.LogisticFunction(theta, Xrows[i]);
                result += Y[i] * Math.Log(p) + (1 - Y[i]) * Math.Log(1 - p);
            }
            return result / Y.Size;
        }

        private static double Func(double intersect, Vector Y)
        {
            double result = 0,
                p = Fn.LogisticFunction(intersect);
            for (int i = 0; i < Y.Size; i++)
                result += Y[i] * Math.Log(p) + (1 - Y[i]) * Math.Log(1 - p);
            return result / Y.Size;
        }

        private static Vector Gradient(Vector theta, Vector[] Xrows, Vector Y)
        {
            int p = theta.Size,
                n = Y.Size;
            Vector result = Vector.Create(p),
                difference = Vector.Create(n);

            for (int i = 0; i < n; i++)
                difference[i] = Y[i] - Fn.LogisticFunction(theta, Xrows[i]);

            for (int k = 0; k < p; k++)
                for (int i = 0; i < n; i++)
                    result[k] += difference[i] * Xrows[i][k] / n;
            return result;
        }

        private static Matrix Hessian(Vector theta, Vector[] Xrows, Vector Y)
        {
            int p = theta.Size,
                n = Y.Size;
            Matrix result = Matrix.Create(p, p);

            Vector h = Vector.Create(n);

            for (int i = 0; i < n; i++)
                h[i] = Fn.LogisticFunction(theta, Xrows[i]);

            for (int k = 0; k < p; k++)
                for (int j = k; j < p; j++)
                {
                    double value = 0;
                    for (int i = 0; i < n; i++)
                        value += Xrows[i][k] * Xrows[i][j] * h[i] * (1 - h[i]);
                    result[k, j] = -value / n;
                    result[j, k] = result[k, j];
                }
            return result;
        }

        #region Regress methods
        /// <summary>Performs the regression using gradient descent</summary>
        public void RegressUsingGradientDescent(double momentum)
        {
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

            #region Perform calculations
            int c = X.Columns;
            Vector initialGuess = Vector.Create(c);
            int[] rowEnumerator = Enumerable.Range(0, n).ToArray();

            Vector[] Xrows = rowEnumerator.Select(i => X.Row(i)).ToArray();

            GradientDescent gd1 = new GradientDescent(initialGuess, LineSearch.Naive, v => Func(v, Xrows, Y), v => Gradient(v, Xrows, Y), OptimizationType.Max, 2000, 100, 1e-4);
            gd1.Optimize(momentum);

            SolverStatus[] acceptableFinalStatuses = new SolverStatus[]
            {
                SolverStatus.FunctionConvergence,
                SolverStatus.StationaryFunction,
                SolverStatus.IterationExceeded
            };


            if (acceptableFinalStatuses.Contains(gd1.Status))
            {
                _status = gd1.Status == SolverStatus.IterationExceeded ? RegressionStatus.IterationExceeded : RegressionStatus.Normal;
                Vector result = gd1.Result;
                _finalLogLikelihood = Func(result, Xrows, Y);
                double r2 = 1 - _finalLogLikelihood / Func(Y.Sum / n, Y);
                if (_withConstant)
                    _logisticModel = new LogisticModel(result[0], result.Data.ToList().GetRange(1, result.Size - 1).ToArray(), r2);
                else
                    _logisticModel = new LogisticModel(0, result.Data, r2);
            }
            else
            {
                _status = RegressionStatus.BadData;
                _finalLogLikelihood = Func(gd1.Result, Xrows, Y);

                if (_returnAverageIfFailed && !_withConstant)
                    _logisticModel = new LogisticModel(Y.Sum / n, 0);
            }
            #endregion
        }

        /// <summary>Performs the regression using conjugate gradient descent</summary>
        public void RegressUsingConjugateGradientDescent()
        {
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

            #region Perform calculations
            int c = X.Columns;
            Vector initialGuess = Vector.Create(c);
            int[] rowEnumerator = Enumerable.Range(0, n).ToArray();

            Vector[] Xrows = rowEnumerator.Select(i => X.Row(i)).ToArray();

            GradientDescent gd1 = new GradientDescent(initialGuess, LineSearch.Naive,
                v => Func(v, Xrows, Y), v => Gradient(v, Xrows, Y), v => Hessian(v, Xrows, Y),
                OptimizationType.Max, 200000 / c, 10, 1e-4);
            gd1.OptimizeConjugate();

            SolverStatus[] acceptableFinalStatuses = new SolverStatus[]
            {
                SolverStatus.FunctionConvergence,
                SolverStatus.StationaryFunction,
                SolverStatus.GradientConvergence,
                SolverStatus.IterationExceeded
            };


            if (acceptableFinalStatuses.Contains(gd1.Status))
            {
                _status = gd1.Status == SolverStatus.IterationExceeded ? RegressionStatus.IterationExceeded : RegressionStatus.Normal;
                Vector result = gd1.Result;
                _finalLogLikelihood = Func(result, Xrows, Y);
                double r2 = 1 - _finalLogLikelihood / Func(Y.Sum / n, Y);

                if (_withConstant)
                    _logisticModel = new LogisticModel(result[0], result.Data.ToList().GetRange(1, result.Size - 1).ToArray(), r2);
                else
                    _logisticModel = new LogisticModel(0, result.Data, r2);
            }
            else
            {
                _status = RegressionStatus.BadData;
                _finalLogLikelihood = Func(gd1.Result, Xrows, Y);

                if (_returnAverageIfFailed && !_withConstant)
                    _logisticModel = new LogisticModel(Y.Sum / n, 0);
            }
            #endregion
        }
        #endregion
    }
}
