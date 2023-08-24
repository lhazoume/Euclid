using Euclid.Analytics.Regressions;
using Euclid.DataStructures.IndexedSeries;
using Euclid.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Euclid.Analytics.Clustering
{
    /// <summary>
    /// Class which modelize a PCA
    /// </summary>
    /// <typeparam name="T">Legends type</typeparam>
    /// <typeparam name="TV">Labels types</typeparam>
    public class PCA<T, TV> where T : IEquatable<T>, IComparable<T> where TV : IEquatable<TV>, IConvertible
    {
        #region vars
        /// <summary>
        /// Number of components
        /// </summary>
        public int C { get; private set; }
        /// <summary>
        /// Covariance matrix
        /// </summary>
        public Matrix Cov { get; private set; }
        /// <summary>
        /// Eigen vectors
        /// </summary>
        public Matrix EigenVectors { get; private set; }
        /// <summary>
        /// Matrix of data
        /// </summary>
        private DataFrame<T, double, TV> _x;
        /// <summary>
        /// Matrix of data transformed by PCA
        /// </summary>
        public DataFrame<T, double, TV> X_ { get; private set; }
        /// <summary>
        /// Eigei values
        /// </summary>
        public Vector EigenValues { get; private set; }
        /// <summary>
        /// Explained variance
        /// </summary>
        public Vector ExplainedVariance { get; private set; }
        /// <summary>
        /// Cumulative variance
        /// </summary>
        public Vector CumulativeVariance { get; private set; }
        /// <summary>
        /// Regression status
        /// </summary>
        public RegressionStatus Status { get; private set; }
        /// <summary>
        /// Apply/Applied centering over data matrix
        /// </summary>
        public bool Centering { get; private set; }
        /// <summary>
        /// Apply/Applied scaling over data matrix
        /// </summary>
        public bool Scaling { get; private set; }
        /// <summary>
        /// Error information about regression
        /// </summary>
        public string Err { get; private set; }
        #endregion

        #region constructor
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="x">Data</param>
        /// <param name="c">Number of components</param>
        /// <param name="centering">Centering</param>
        /// <param name="scaling">Scaling</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        private PCA(DataFrame<T, double, TV> x, int c, bool centering, bool scaling)
        {
            if (x == null) throw new ArgumentNullException(nameof(x), "the x should not be null");
            if (x.Columns == 0) throw new ArgumentException("the data is not consistent");
            if(c > x.Columns) throw new ArgumentException($"The number of components has to be inferior or equals to the number of dimension ({c} <= {x.Columns})");

            _x = x.Clone<DataFrame<T, double, TV>>();

            C = c < 0 ? _x.Columns : c;

            Centering = centering;
            Scaling = scaling;

            Status = RegressionStatus.NotRan;
        }

        #endregion

        #region methods

        #region create
        /// <summary>
        /// Create function
        /// </summary>
        /// <param name="x">Data</param>
        /// <param name="centering">Centering</param>
        /// <param name="scaling">Scaling</param>
        /// <param name="c">Number of components</param>
        /// <returns>PCA object</returns>
        public static PCA<T, TV> Create(DataFrame<T, double, TV> x, bool centering = true, bool scaling = true,int c = -1) { return new PCA<T, TV>(x, c, centering, scaling); }
        #endregion

        /// <summary>
        /// Fit a PCA
        /// </summary>
        public void Fit()
        {
            try
            {
                #region pre-process data by scaling
                Matrix X = Matrix.Create(_x.Data);
                Scaling xScaler = Regressions.Scaling.CreateZScore(X);
                if (xScaler == null)
                {
                    Status = RegressionStatus.BadData;
                    return;
                }

                Matrix Xs = xScaler.Reduce(X, Centering, Scaling);
                #endregion

                #region compute COV matrix Xt.X
                Matrix cov = Matrix.FastTransposeBySelf(X);
                #endregion

                #region perform eigen decomposition of cov matrix
                EigenDecomposition eigen = new EigenDecomposition(cov);
                Vector[] eigeiVectors = eigen.RealEigenVectors;
                EigenValues = Vector.Create(eigen.RealEigenValues);
                #endregion

                #region sort eigen values and corresponding eigen vectors in descending order
                IReadOnlyList<int> indices = Vector.Sort(EigenValues, true);
                Vector[] eigeiVectorsSorted = new Vector[indices.Count];

                for (int i = 0; i < indices.Count; i++) eigeiVectorsSorted[i] = eigeiVectors[indices[i]];
                EigenVectors = Matrix.Create(eigen.RealEigenVectors);
                #endregion

                #region compute explained variance
                double sumEigeiVal = EigenValues.Sum;
                ExplainedVariance = EigenValues / sumEigeiVal;
                CumulativeVariance = Vector.Cumsum(EigenValues);
                #endregion

                #region transform X
                Matrix x_ = Xs * EigenVectors;
                X_ =  DataFrame<T, double, TV>.Create<DataFrame<T, double, TV>>(_x.Labels, _x.Legends, x_.JaggedArray);
                #endregion

                Status = RegressionStatus.Normal;
            }

            catch(Exception ex)
            {
                Status = RegressionStatus.BadData;
                Err = $"PCA.Fit: {ex.Message}";
            }
        }
        #endregion
    }
}
