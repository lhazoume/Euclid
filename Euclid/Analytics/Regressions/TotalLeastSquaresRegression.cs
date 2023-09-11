using Euclid.DataStructures.IndexedSeries;
using Euclid.Extensions;
using Euclid.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Euclid.Analytics.Regressions
{
    /// <summary>
    /// Total Least Squares Regression class
    /// </summary>
    public class TotalLeastSquaresRegression
    {
        #region vars
        /// <summary>
        /// used Constant into error measurement
        /// </summary>
        public bool WithConstant { get; private set; }

        /// <summary>
        /// Enable error computation
        /// </summary>
        public bool ComputeErr { get; private set; }

        /// <summary>
        /// Regression status
        /// </summary>
        public RegressionStatus Status { get; private set; }

        /// <summary>
        /// Data to regress
        /// </summary>
        public  Matrix X { get; private set; }

        /// <summary>
        /// Coefficient of the regression, bo is intercept
        /// </summary>
        public Vector Beta { get; private set; }

        /// <summary>
        /// Linear attributes of the regression
        /// </summary>
        public LinearModel Linear { get; private set; }
        #endregion

        #region constructor
        /// <summary>
        /// TLS constructor
        /// </summary>
        /// <param name="x">Data</param>
        /// <param name="withConstant">With contant</param>
        /// <param name="computeErr">Compute error</param>
        private TotalLeastSquaresRegression(Matrix x, bool withConstant, bool computeErr)
        {
            WithConstant = withConstant;
            ComputeErr = computeErr;
            Status = RegressionStatus.NotRan;
            X = x;
        }
        #endregion

        #region methods

        #region create
        /// <summary>
        /// Create a TLS from a matrix
        /// </summary>
        /// <param name="x">Data</param>
        /// <param name="withConstant">With contant</param>
        /// <param name="computeErr">Compute error</param>
        /// <returns>TLS objects</returns>
        public static TotalLeastSquaresRegression Create(Matrix x, bool withConstant = true, bool computeErr = true) { return new TotalLeastSquaresRegression(x, withConstant, computeErr); }

        /// <summary>
        /// Create a TLS from a jagged array
        /// </summary>
        /// <param name="x">Data</param>
        /// <param name="withConstant">With contant</param>
        /// <param name="computeErr">Compute error</param>
        /// <returns>TLS objects</returns>
        public static TotalLeastSquaresRegression Create(double[][] x, bool withConstant = true, bool computeErr = true) { return new TotalLeastSquaresRegression(Matrix.Create(x), withConstant, computeErr); }

        /// <summary>
        /// Create a TLS from a dataframe
        /// </summary>
        /// <typeparam name="T">Legend</typeparam>
        /// <typeparam name="TV">Label</typeparam>
        /// <param name="x">Data</param>
        /// <param name="withConstant">With contant</param>
        /// <param name="computeErr">Compute error</param>
        /// <returns>TLS objects</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static TotalLeastSquaresRegression Create<T, TV>(IDataFrame<T, double, TV> x, bool withConstant = true, bool computeErr = true) where T : IEquatable<T>, IComparable<T> where TV : IEquatable<TV>, IConvertible
        {
            #region requirements
            if (x == null) throw new ArgumentNullException(nameof(x));
            if (x.Columns == 0 || x.Rows == 0) throw new ArgumentException("the data is not consistent");
            #endregion

            return Create(x.Data, withConstant, computeErr); 
        }
        #endregion


        /// <summary>
        /// Run the Total Least Squares regression
        /// </summary>
        public void Regress()
        {
            SingularValueDecomposition svd = SingularValueDecomposition.Run(X, SVDType.POWER_ITERATION);

            Matrix V = svd.V;
            int n = V.Rows - 1, p = V.Columns - 1;

            Beta = Vector.Create(n);

            for(int i = 0; i < n; i++) Beta[i] = (-1*V[i,p]) / V[n,p];

            #region compute error
            double ssr = 0, sse = 0, y_ = 0, x_ = 0;
            Matrix cov = null;
            if (ComputeErr)
            {
                for (int i = 0; i < X.Rows; i++)
                {
                    for (int j = 0; j < X.Columns - 1; j++) x_ += X[i, j];
                    y_ += X[i, p];
                }
                y_ /= (X.Rows * 1.0);
                x_ /= (X.Rows * 1.0);

                double x_squares = 0, y_squares = 0;
                for (int i = 0; i < X.Rows; i++)
                {
                    double yhat = 0;
                    for (int j = 0; j < X.Columns - 1; j++)
                    {
                        yhat += X[i, j] * Beta[j];
                        x_squares += Math.Pow(X[i, j] - x_, 2.0);
                    }

                    y_squares += Math.Pow(X[i, p] - y_, 2.0);

                    ssr += Math.Pow(yhat - y_, 2);
                    sse += Math.Pow(X[i, p] - yhat, 2);
                }

                #region compute correlation
                cov = Matrix.FastTransposeBySelf(X);
                double x_sigma = Math.Sqrt(x_squares), y_sigma = Math.Sqrt(y_squares);
                cov = cov / (x_sigma * y_sigma);
                #endregion

                #region compute constant
                double alpha = 0;
                if (WithConstant)
                {
                    for (int j = 0; j < X.Columns - 1; j++) alpha -= Beta[j] * x_;
                    alpha += y_;
                }
                #endregion

                Linear = new LinearModel(alpha, Beta.Data, cov.Data, X.Rows, sse, ssr);
            }
            #endregion

            Status = RegressionStatus.Normal;
        }

        #endregion
    }
}
