using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Euclid.IndexedSeries.Analytics.Regressions
{
    public class LASSORegression<T, V> where T : IEquatable<T>, IComparable<T> where V : IEquatable<V>, IConvertible
    {
        #region Declarations
        private bool _returnAverageIfFailed, _withConstant, _computeErr;
        private double _regularization;
        private RegressionStatus _status;
        private LinearModel _linearModel = null;
        private DataFrame<T, double, V> _x;
        private Series<T, double, V> _y;
        #endregion

        public LASSORegression(DataFrame<T, double, V> x, Series<T, double, V> y, double regularization)
        {
            if (x == null || y == null) throw new ArgumentNullException("the x and y should not be null");
            if (x.Columns == 0 || x.Rows != y.Rows) throw new ArgumentException("the data is not consistent");
            if (regularization <= 0) throw new ArgumentException("the regularization factor should be positive");

            _x = x.Clone();
            _y = y.Clone();
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
            double precision = 10e-3;
            while ((W - WOld).NormSup > precision)
            {
                WOld = W;
                W = IntermediateStepShootingLASSO(tXY, tXX, W);
            }
            # endregion

            Parallel.For(0, W.Size, i => { if (Math.Abs(W[i]) < 10e-3) W[i] = 0; });

            return W;
        }

        private LinearModel LinearLASSORegs(DataFrame<T, double, V> x, Series<T, double, V> y)
        {
            LinearModel result = new LinearModel();
            if (x.Rows <= 1) return result;
            if (x.Rows < x.Columns) return result;

            int n = x.Rows, p = x.Columns;
            Matrix X = Matrix.Create(x.Data);

            #region X Scaling

            Scaling[] scalings = MatrixColumnWiseScaler(X);
            Matrix Xr = MatrixColumnWiseReducer(X, scalings);

            #endregion

            #region Inversion
            Matrix tXr = Xr.FastTranspose,
                tXrXr = Matrix.FastTransposeBySelf(Xr),
                initializeW = Matrix.LinearCombination(1, tXrXr, _regularization, Matrix.CreateIdentityMatrix(tXrXr.Rows, tXrXr.Columns)).FastInverse;

            if (initializeW == null) return result;
            #endregion

            #region Reduce the Y
            Vector Y = Vector.Create(y.Data);
            Scaling Yscaler = Scaling.CreateZScore(y.Data);

            if (Yscaler == null) return result;

            Vector Yr = Vector.Create(Yscaler.Reduce(y.Data));

            #endregion

            #region Y Matrix Operations
            Vector tXY = tXr * Yr,
                W0 = initializeW * tXY;

            #endregion

            #region Calculate

            Vector W = LASSOGradientDescent(tXY, tXrXr, W0);

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

            #region Quality & result
            double r2 = 0, adjR2 = 0, SSres = 0;

            if (_computeErr)
            {
                #region Evaluates the error quality indicators
                Vector residuals = Y - (X * W) - b,
                    deviation = Y - Yscaler.Intercept;

                SSres = residuals.SumOfSquares;
                double SStot = deviation.SumOfSquares;

                int nonZeroCoefficients = 0;
                for (int k = 0; k < W.Size; k++) if (W[k] != 0) nonZeroCoefficients++;

                int boundCols = X.Rows - nonZeroCoefficients - 1;
                r2 = SStot == 0 ? 0 : 1 - SSres / SStot;
                adjR2 = SStot == 0 ? 0 : (boundCols == 0 ? 0 : r2 - (1 - r2) * nonZeroCoefficients / boundCols);
                #endregion
            }
            result = new LinearModel(b, W.Data, new double[W.Size], n, SSres, SSres);
            #endregion


            #endregion

            return result;
        }
    }
}
