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

            Matrix V = svd.V.Transpose;
            int n = V.Rows - 1, n_1 = n - 1, p = V.Columns - 1;

            Beta = Vector.Create(n);

            for(int i = 0; i < n; i++) Beta[i] = (-1*V[i,p]) / V[n_1,p];

            Status = RegressionStatus.Normal;
        }

        #endregion
    }
}
