using Euclid.IndexedSeries;
using Euclid.Solvers;
using System;
using System.Threading.Tasks;

namespace Euclid.Analytics.Regressions
{
    /// <summary>
    /// Performs a LASSO regression for a given regularization factor
    /// </summary>
    /// <typeparam name="T">the legends</typeparam>
    /// <typeparam name="V">the labels</typeparam>
    public class LASSORegression<T, V> where T : IEquatable<T>, IComparable<T> where V : IEquatable<V>, IConvertible
    {
        #region Declarations
        private bool _computeErr;
        private double _regularization;
        private RegressionStatus _status;
        private LinearModel _linearModel = null;
        private DataFrame<T, double, V> _x;
        private Series<T, double, V> _y;
        #endregion

        /// <summary>Buils a LASSO to regress a <c>Series</c> on a <c>DataFrame</c></summary>
        /// <param name="x">the <c>DataFrame</c></param>
        /// <param name="y">the <c>Series</c></param>
        /// <param name="regularization">the regularization factor</param>
        public LASSORegression(DataFrame<T, double, V> x, Series<T, double, V> y, double regularization)
        {
            if (x == null) throw new ArgumentNullException("x", "the x should not be null");
            if (y == null) throw new ArgumentNullException("y", "the y should not be null");
            if (x.Columns == 0 || x.Rows != y.Rows) throw new ArgumentException("the data is not consistent");
            if (regularization <= 0) throw new ArgumentException("the regularization factor should be positive");

            _x = x.Clone();
            _y = y.Clone();
            _computeErr = true;
            _regularization = regularization;
            _status = RegressionStatus.NotRan;
        }

        #region  Accessors

        #region Settables
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

        private static Scaling[] MatrixColumnWiseScaler(Matrix X)
        {
            Scaling[] scalers = new Scaling[X.Columns];

            Parallel.For(0, X.Columns, i => { scalers[i] = Scaling.CreateZScore(X.Column(i).Data); });

            return scalers;
        }
        private static Matrix MatrixColumnWiseReducer(Matrix A, Scaling[] scalers)
        {
            Vector[] vectors = new Vector[A.Columns];
            Parallel.For(0, A.Columns, i =>
            {
                if (scalers[i] == null) return;
                vectors[i] = Vector.Create(scalers[i].Reduce(A.Column(i).Data));
            });

            return Matrix.CreateFromColumns(vectors);
        }
        private Vector IntermediateStepShootingLASSO(Vector tXY, Matrix tXX, Vector W)
        {
            Vector result = Vector.Create(W.Size);
            Parallel.For(0, W.Size, j =>
            {
                double currentSum = -tXY[j];
                for (int i = 0; i < W.Size; i++)
                    if (i != j)
                        currentSum += tXX[i, j] * W[i];

                if (currentSum > _regularization)
                    result[j] = (_regularization - currentSum) / tXX[j, j];
                else if (currentSum < -_regularization)
                    result[j] = (-_regularization - currentSum) / tXX[j, j];
            });
            return result;
        }
        private Vector LASSOGradientDescent(Vector tXY, Matrix tXX, Vector Wi)
        {
            Vector W = Wi.Clone;

            # region Performs the shrink and shoot gradient descent
            Vector WOld = Vector.Create(W.Size);
            while ((W - WOld).NormSup > Descents.ERR_EPSILON)
            {
                WOld = W;
                W = IntermediateStepShootingLASSO(tXY, tXX, W);
            }
            # endregion

            Parallel.For(0, W.Size, i => { if (Math.Abs(W[i]) < Descents.ERR_EPSILON) W[i] = 0; });

            return W;
        }

        /// <summary>Performs the regression</summary>
        public void Regress()
        {
            _linearModel = new LinearModel();

            int n = _x.Rows, p = _x.Columns;

            #region Read and Reduce the data
            Matrix X = Matrix.Create(_x.Data);
            Scaling[] scalings = MatrixColumnWiseScaler(X);
            Matrix Xr = MatrixColumnWiseReducer(X, scalings);

            Vector Y = Vector.Create(_y.Data);
            Scaling Yscaler = Scaling.CreateZScore(_y.Data);
            if (Yscaler == null)
            {
                _status = RegressionStatus.BadData;
                return;
            }
            Vector Yr = Vector.Create(Yscaler.Reduce(_y.Data));
            double yb = Y.Sum / n,
                sst = _computeErr ? Y.SumOfSquares - n * yb * yb : 0;
            #endregion

            #region Perform the LASSO
            Matrix tXr = Xr.FastTranspose,
                tXrXr = Matrix.FastTransposeBySelf(Xr),
                initializeW = Matrix.LinearCombination(1, tXrXr, _regularization, Matrix.CreateIdentityMatrix(tXrXr.Rows, tXrXr.Columns)).FastInverse;

            if (initializeW == null)
            {
                _status = RegressionStatus.BadData;
                return;
            }
            Vector tXY = tXr * Yr,
                W0 = initializeW * tXY;

            Vector W = LASSOGradientDescent(tXY, tXrXr, W0);
            #endregion

            #region Rescales the coefficients and the data
            W = W * Yscaler.ScalingCoefficient;
            double tXavgW = 0;
            for (int j = 0; j < W.Size; j++)
            {
                W[j] /= scalings[j].ScalingCoefficient;
                tXavgW += W[j] * scalings[j].Intercept;
            }
            double b = Yscaler.Intercept - tXavgW;
            #endregion

            #region Output
            double sse = 0;
            double[] correls = new double[p];
            if (_computeErr)
            {
                #region Error
                Vector residuals = Y - (X * W) - b;
                sse = residuals.SumOfSquares;
                #endregion

                #region Correlations
                Vector cov = X.Transpose * Y;
                Matrix tXX = Matrix.FastTransposeBySelf(X);
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

            _linearModel = new LinearModel(b, W.Data, correls, n, sse, sst);
            _status = RegressionStatus.Normal;
        }
    }
}
