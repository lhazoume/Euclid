using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Euclid.LinearAlgebra
{
    /// <summary>
    /// Enumeration which describes SVD type
    /// </summary>
    public enum SVDType
    {
        JACOBI = 0,
        POWER_ITERATION = 1
    }
    /// <summary>
    /// Generalizes the eigendecomposition of a square normal matrix with an orthogonal eigenbasis (A = U.D.V^t)
    /// </summary>
    public class SingularValueDecomposition
    {
        #region vars
        public Matrix A { get; private set; }
        public Matrix V { get; private set; }
        public Matrix U { get; private set; }
        public Vector D { get; private set; }
        #endregion

        #region constructor
        private SingularValueDecomposition(Matrix a)
        {
            A = a.Clone;
            V = Matrix.CreateIdentityMatrix(A.Columns, A.Rows);
            U = Matrix.Create(A.Rows, A.Rows);
            D = Vector.Create(A.Columns, 1);
        }
        #endregion

        #region methods
        /// <summary>
        /// Run a Singular Value Decomposition
        /// </summary>
        /// <param name="a">Matrix</param>
        /// <param name="svdType">SVD transformation type used for Eigen decomposition, Jacobi by default.</param>
        /// <param name="epsilon">tolerance for power iteration</param>
        /// <returns>SVD object</returns>
        /// <exception cref="Exception"></exception>
        public static SingularValueDecomposition Run(Matrix a, SVDType svdType = SVDType.JACOBI, double epsilon = 1e-10)
        {
            if (a == null) throw new ArgumentNullException("a");

            SingularValueDecomposition svd = new SingularValueDecomposition(a);
            Matrix A = svd.A;

            #region run svd
            switch(svdType)
            {
                default: case SVDType.JACOBI: svd.SVDbyJacobi(); break;
                case SVDType.POWER_ITERATION: svd.SVDbyPowerIteration(epsilon); break;
            }
            #endregion 

            return svd; 
        }

        #region Jacobi transform
        /// <summary>
        /// Process SVD by using Jacobi transformation
        /// </summary>
        /// <param name="C">Symmetric matrix</param>
        private void SVDbyJacobi()
        {
            #region compute A^T.A
            Matrix C = A.Transpose * A;

            if (!C.IsSymmetric) throw new Exception("Impossible to lead an eigen decomposition by Jacobi transform due to A^t.A is not symmetric");
            #endregion

            #region compute eigeivalues & vectors by jacobi
            Matrix R = JacobiTransform(C, C.Columns);
            #endregion

            #region compute D
            int m = C.Columns;
            for (int i = 0; i < m; i++)
            {
                if (C[i, i] > 0) D[i] = Math.Sqrt(C[i, i]);
                else D[i] = 0;
            }
            #endregion

            #region order values
            for (int i = 0; i < m - 1; i++)
            {
                int idx = i;
                for (int j = i + 1; j < m; j++)
                {

                    if (D[j] > D[idx])
                        idx = j;
                }

                double tempVal = D[i];
                D[i] = D[idx];
                D[idx] = tempVal;
                for (int j = 0; j < m; j++)
                {
                    tempVal = V[j, i];
                    V[j, i] = V[j, idx];
                    V[j, idx] = tempVal;
                }
            }

            int rank = 1;
            for (int i = 0; i < m; i++)
                if (Math.Abs(D[i]) > 1E-5) 
                    rank = i + 1;
            #endregion

            #region compute U
            int n = V.Columns;
            for (int i = 0; i < rank; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    U[j, i] = 0;
                    for (int k = 0; k < m; k++)
                    {
                        U[j, i] = U[j, i] + A[j, k] * V[k, i];
                    }
                    if (D[i] != 0) U[j, i] = U[j, i] / D[i];
                }
            }

            #region fill empty columns of U
            if ((rank > 1) && (rank < n)) FillCols(n - rank);
            #endregion
            #endregion
        }

        /// <summary>
        /// Transform a symmetric matrix C into a diagonal matrix by the use of orthogonal matrixes (rotation matrixes)
        /// </summary>
        ///<param name="C">symmetric matrix C </param>
        /// <param name="O">Maximal order</param>
        /// <param name="epsilon">minimal increment</param>
        /// <param name="maxIter">Iteration maximal</param>
        /// <returns>Rotation matrix</returns>
        private Matrix JacobiTransform(Matrix C, int O, int maxIter = 50, double epsilon = 1.0E-40)
        {
            int i, k, count = 0;
            double delta = 10;
            Matrix R = Matrix.Create(A.Rows, A.Rows);
            
            while ((delta > epsilon) && (count < maxIter))
            {
                delta = 0;
                count++;
                for (i = 0; i < O; i++)
                {
                    for (k = i + 1; k < O; k++)
                    {
                        if (C[k, i] != 0.0)
                        {
                            JacobiRotationMatrix(R, C, i, k, O);
                            Matrix.MultTranspLeft(C, R, i, k, O);
                            Matrix.MultRight(C, R, i, k, O);
                            Matrix.MultRight(V, R, i, k, O);
                        }
                    }
                    if (i < O - 1)
                        delta = delta + Math.Abs(C[i + 1, i]);
                }
            }

            return R;
        }

        /// <summary>
        /// Compute the Jacobi's rotation matrix
        /// </summary>
        /// <param name="r">Rotation matrix</param>
        /// <param name="a">Symmetric matrix</param>
        /// <param name="p">p row</param>
        /// <param name="q">q row</param>
        /// <param name="size">size</param>
        private void JacobiRotationMatrix(Matrix r, Matrix a, int p, int q, int size)
        {
            #region requirements
            int i, j;
            double phi, t;
            #endregion

            phi = (a[q, q] - a[p, p]) / 2 / a[q, p];

            if (phi != 0)
            {
                if (phi > 0)
                    t = 1 / (phi + Math.Sqrt(phi * phi + 1.0));
                else
                    t = 1 / (phi - Math.Sqrt(phi * phi + 1.0));
            }
            else t = 1.0;

            for (i = 0; i < size; i++)
            {
                for (j = 0; j < size; j++)
                {
                    r[i, j] = 0.0;
                }
                r[i, i] = 1.0;
            }
            r[p, p] = 1 / Math.Sqrt(t * t + 1.0);
            r[p, q] = t * r[p, p];
            r[q, q] = r[p, p];
            r[q, p] = -r[p, q];
        }

        /// <summary>
        /// Fill empty cols by using Gauss algorithm
        /// </summary>
        /// <param name="empties">Nb of elt to fill</param>
        private void FillCols(int empties)
        {
            int n = A.Rows;
            GaussEliminationAlgorithm Gauss = GaussEliminationAlgorithm.Create(A.Rows);

            for (int index = 0; index < empties; index++)
            {
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        Gauss.A[j, i] = U[i, j];
                    }
                    Gauss.y[i] = 0;
                }

                Gauss.Eliminate(empties);
                Gauss.Solve(empties - index);

                for (int i = 0; i < Gauss.N; i++) U[i, n - empties + index] = Gauss.x[i];
            }
        }
        #endregion

        #region power iteration
        /// <summary>
        /// Compute SVD method by using Power iteration from Risto Hinno implementations (Naive ways to calculate SVD) : https://towardsdatascience.com/simple-svd-algorithms-13291ad2eef2 & https://github.com/RRisto/learning/blob/master/linear_algebra_learn/PCA_SVD/power_method.ipynb
        /// </summary>
        /// <param name="epsilon">Tolerance</param>
        private void SVDbyPowerIteration(double epsilon = 1e-10)
        {
            #region requirements
            int n = A.Rows, m = A.Columns, k = Math.Min(n, m);

            List<Tuple<Vector, Vector, double>> R = new List<Tuple<Vector, Vector, double>>();
            #endregion

            #region compute singular value & vector per power iteration
            for (int i = 0; i < k; i++)
            {
                Matrix A_ = A.Clone;

                for(int l = 0;  l < R.Count; l++)
                    A_ = A_ - (R[l].Item3 * (R[l].Item2 * R[l].Item1));

                Vector V, U;
                double Ev, d;

                if( n > m)
                {
                    DominantEigen(A_, out V, out Ev, epsilon); // compute singular vector V
                    Vector U_ = A * V; // U unnormalized
                    d = U_.Norm2; // singular value d
                    U = U_ / d;
                }

                else
                {
                    DominantEigen(A_, out U, out Ev, epsilon); // compute singular vector V
                    Vector V_ = A.Transpose * U;
                    d = V_.Norm2;
                    V = V_ / d;
                }

                R.Add(new Tuple<Vector, Vector, double>(V, U, d));
            }
            #endregion

            #region sort singular vector per descending singular values
            R.Sort((x, y) => -1 * x.Item3.CompareTo(y.Item3));
            #endregion

            U = Matrix.Create(R.Select(r => r.Item2).ToList());
            V = Matrix.Create(R.Select(r => r.Item1).ToList());
            D = Vector.Create(R.Select(r => r.Item3).ToList());
        }

        /// <summary>
        /// Compute the dominant eigen
        /// </summary>
        /// <param name="a">Matrix</param>
        /// <param name="V">Singular vector</param>
        /// <param name="Ev">Eigen value</param>
        /// <param name="epsilon">Tolerance</param>
        private void DominantEigen(Matrix a, out Vector V, out double Ev, double epsilon = 1e-10)
        {
            #region requirements
            int n = a.Rows, m = a.Columns, k = Math.Min(n, m);

            Vector v = Vector.Create(k, 1) / Math.Sqrt(k);

            if (n > m) a = a.Transpose * a;
            else if (n < m) a = a * a.Transpose;

            double ev = ComputeEigenValue(a, v);
            #endregion

            #region compute most dominant eigenvalue & eigenvector
            while(true)
            {
                Vector av = a * v;
                V = av / av.Norm2;
                Ev = ComputeEigenValue(a, V);

                if (Math.Abs(ev - Ev) <= epsilon) return;

                v = V;
                ev = Ev;
            }
            #endregion
        }

        /// <summary>
        /// Compute eigen value
        /// </summary>
        /// <param name="a">Matrix</param>
        /// <param name="v">Singular vector</param>
        /// <returns></returns>
        private double ComputeEigenValue(Matrix a, Vector v)
        {
            Vector v_ = (a * v ) / v;
            return v_[0];
        }
        #endregion

        #endregion
    }
}
