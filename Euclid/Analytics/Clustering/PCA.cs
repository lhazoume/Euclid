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
        /// Variance threshold which defines the number of components included at the barrier
        /// </summary>
        public double W { get; private set; }
        /// <summary>
        /// Number of components according the variance threshold
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
        /// <param name="centering">Centering</param>
        /// <param name="scaling">Scaling</param>
        /// <param name="w">Variance threshold (< 1 )</param>
        private PCA(DataFrame<T, double, TV> x, bool centering, bool scaling, double w)
        {
            if (x == null) throw new ArgumentNullException(nameof(x), "the x should not be null");
            if (x.Columns == 0) throw new ArgumentException("the data is not consistent");
            if (W >= 1) throw new ArgumentException("Inefficient variance threshold, w < 1");

            _x = x.Clone<DataFrame<T, double, TV>>();

            Centering = centering;
            Scaling = scaling;

            W = w;

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
        /// <param name="w">Variance threshold</param>
        /// <returns>PCA object</returns>
        public static PCA<T, TV> Create(DataFrame<T, double, TV> x, bool centering = true, bool scaling = true, double w = 0.5) { return new PCA<T, TV>(x, centering, scaling, w); }
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

                #region compute COV tXs.Xs
                Cov = Matrix.FastTransposeBySelf(Xs);
                #endregion

                #region perform eigen decomposition of cov matrix
                EigenDecomposition eigen = new EigenDecomposition(Cov);
                Vector[] eigeiVectors = eigen.RealEigenVectors;
                Vector eigenValues = Vector.Create(eigen.RealEigenValues);
                #endregion

                #region sort eigen values and corresponding eigen vectors in descending order
                int[] indices;
                EigenValues = Vector.Sort(eigenValues, out indices, true);
                Vector[] eigeiVectorsSorted = new Vector[eigenValues.Size];

                for (int i = 0; i < eigenValues.Size; i++) eigeiVectorsSorted[i] = eigeiVectors[indices[i]];
                EigenVectors = Matrix.Create(eigeiVectorsSorted);
                #endregion

                #region compute explained variance
                double sumEigenVal = EigenValues.Sum;
                ExplainedVariance = EigenValues / sumEigenVal;
                CumulativeVariance = Vector.Cumsum(ExplainedVariance);
                #endregion

                #region define the # of components filtering by the var threshold
                for(int i = 0; i < CumulativeVariance.Size; i++)
                    if(CumulativeVariance[i] > W)
                    {
                        C = i + 1;
                        break;
                    }
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
