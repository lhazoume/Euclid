using Euclid.Analytics.Regressions;
using Euclid.DataStructures.IndexedSeries;
using Euclid.Extensions;
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
    public class PCA
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
        private double[][] _x;
        /// <summary>
        /// Matrix of data transformed by PCA
        /// </summary>
        public double[][] X_ { get; private set; }
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
        /// <summary>
        /// Adjusting the eigenvectors that are largest in absolute value to be positive
        /// </summary>
        public bool AdjustEigenVectors { get; private set; }
        #endregion

        #region constructor
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="x">Data</param>
        /// <param name="centering">Centering</param>
        /// <param name="scaling">Scaling</param>
        /// <param name="w">Variance threshold (< 1 )</param>
        /// <param name="adjustEigenVectors">Adjusting the eigen vectors</param>
        /// <param name="deepCopy">Release a deep copy of the data</param>
        private PCA(double[][] x, bool centering, bool scaling, double w, bool adjustEigenVectors, bool deepCopy = false)
        {
            if (x == null) throw new ArgumentNullException(nameof(x), "the x should not be null");
            if (x.Length == 0) throw new ArgumentException("the data is not consistent, no rows");
            if (x.First().Length == 0) throw new ArgumentException("the data is not consistent, no columns");
            if (W >= 1) throw new ArgumentException("Inefficient variance threshold, w < 1");

            _x = deepCopy? x: Arrays.Clone(x);

            Centering = centering;
            Scaling = scaling;
            AdjustEigenVectors = adjustEigenVectors;
            W = w;

            Status = RegressionStatus.NotRan;
        }

        #endregion

        #region methods

        #region create
        /// <summary>
        /// Create function
        /// </summary>
        /// <param name="x">Dataframe</param>
        /// <param name="centering">Centering</param>
        /// <param name="scaling">Scaling</param>
        /// <param name="w">Variance threshold</param>
        /// <param name="adjustEigenVectors">Adjusting the eigen vectors</param>
        /// <param name="deepCopy">Release a deep copy of the data</param>
        /// <returns>PCA object</returns>
        public static PCA Create<T, TV>(IDataFrame<T, double, TV> x, bool centering = true, bool scaling = true, double w = 0.5, bool adjustEigenVectors = false, bool deepCopy = false) where T : IEquatable<T>, IComparable<T> where TV : IEquatable<TV>, IConvertible
        { return new PCA(x.Data, centering, scaling, w, adjustEigenVectors, deepCopy); }

        /// <summary>
        /// Create function
        /// </summary>
        /// <param name="x">Data</param>
        /// <param name="centering">Centering</param>
        /// <param name="scaling">Scaling</param>
        /// <param name="w">Variance threshold</param>
        /// <param name="adjustEigenVectors">Adjusting the eigen vectors</param>
        /// <param name="deepCopy">Release a deep copy of the data</param>
        /// <returns>PCA object</returns>
        public static PCA Create(double[][] x, bool centering = true, bool scaling = true, double w = 0.5, bool adjustEigenVectors = false, bool deepCopy = false) { return new PCA(x, centering, scaling, w, deepCopy); }
        #endregion

        /// <summary>
        /// Fit a PCA
        /// </summary>
        public void Fit()
        {
            try
            {
                #region pre-process data by scaling
                Matrix X = Matrix.Create(_x);
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

                #region adjusting eigenVectors
                if(AdjustEigenVectors)
                {
                    #region identify max arg of absolutes values per row & get its sign
                    int L = eigeiVectors.Length, H = eigeiVectors.First().Size;

                    double[] signs = new double[H];
                    for(int i = 0; i < H; i++)
                    {
                        double absmaxRow = double.MinValue;
                        for (int j = 0; j < L; j++)
                        {
                            double abs = Math.Abs(eigeiVectors[j][i]);
                            if (absmaxRow >= abs) continue;
                            
                            absmaxRow = abs;
                            signs[i] = eigeiVectors[j][i] / absmaxRow;
                        }
                    }

                    for (int i = 0; i < H; i++)
                        for(int j = 0; j < L; j++)
                            eigeiVectors[j][i] *= signs[i];
                    #endregion

                    #region transpose collection of vector
                    Vector[] V = new Vector[H];
                    for(int i =0; i < H; i++)
                    {
                        Vector Vr = Vector.Create(L, 0);
                        for (int j = 0; j < L; j++) Vr[j] = eigeiVectors[j][i];
                        V[i] = Vr;
                    }
                    eigeiVectors = V;
                    #endregion
                }
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
                X_ = x_.JaggedArray;
                #endregion

                Status = RegressionStatus.Normal;
            }

            catch(Exception ex)
            {
                Status = RegressionStatus.BadData;
                Err = $"PCA.Fit: {ex.Message}";
            }
        }

        /// <summary>
        /// Compute the Total Least Squared coefficients from eigen vectors
        /// </summary>
        /// <returns>TLS Coefficients (slope eq beta)</returns>
        public Vector GetTLSCoefficients()
        {
            #region computation extract from: https://stats.stackexchange.com/questions/13152/how-to-perform-orthogonal-regression-total-least-squares-via-pca
            // Bk = -Vk / Vp+1

            if (EigenVectors.Rows > 2) throw new Exception($"the method cannot compute TLS coefficient from PCA for more than 2 variables");

            Matrix V = EigenVectors;

            int N = V.Rows - 1, N_1 = V.Rows - 1, m = V.Columns - 1;
            double[] B = new double[N_1];

            for (int i = 0; i < N_1; i++) B[i] = (-1.0 * V[i, m]) / V[N_1, m];

            #endregion

            return Vector.Create(B);
        }
        #endregion
    }
}
