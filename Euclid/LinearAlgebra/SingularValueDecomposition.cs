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
        public Matrix D { get; private set; }
        #endregion

        #region constructor
        private SingularValueDecomposition(Matrix a)
        {
            A = a.Clone;
            V = Matrix.CreateIdentityMatrix(A.Columns, A.Rows);
            U = Matrix.Create(A.Rows, A.Rows);
            D = Matrix.Create(A.Columns, 1);
        }
        #endregion

        #region methods
        /// <summary>
        /// Run a Singular Value Decomposition
        /// </summary>
        /// <param name="a">Matrix</param>
        /// <param name="svdType">SVD transformation type used for Eigen decomposition, Jacobi by default.</param>
        /// <returns>SVD object</returns>
        /// <exception cref="Exception"></exception>
        public static SingularValueDecomposition Run(Matrix a, SVDType svdType = SVDType.JACOBI)
        {
            if (a == null) throw new ArgumentNullException("a");

            SingularValueDecomposition svd = new SingularValueDecomposition(a);
            Matrix A = svd.A;

            #region compute A^T.A
            Matrix C = A.Transpose * A;
            #endregion

            #region run svd
            switch(svdType)
            {
                default: case SVDType.JACOBI:
                    if (!C.IsSymmetric) throw new Exception("Impossible to lead an eigen decomposition by Jacobi transform due to A^t.A is not symmetric");
                    svd.SVDbyJacobi(C);
                break;
            }
            #endregion 

            return svd; 
        }

        /// <summary>
        /// Process SVD by using Jacobi transformation
        /// </summary>
        /// <param name="C">Symmetric matrix</param>
        private void SVDbyJacobi(Matrix C)
        {
            #region compute eigeivalues & vectors by jacobi
            Matrix R = JacobiTransform(C, C.Columns);
            #endregion

            #region compute D
            int m = C.Columns;
            for (int i = 0; i < m; i++)
            {
                if (C[i, i] > 0) D[i,0] = Math.Sqrt(C[i, i]);
                else D[i,0] = 0;
            }
            #endregion

            #region order values
            for (int i = 0; i < m - 1; i++)
            {
                int idx = i;
                for (int j = i + 1; j < m; j++)
                {

                    if (D[j, 0] > D[idx, 0])
                        idx = j;
                }

                double tempVal = D[i, 0];
                D[i, 0] = D[idx, 0];
                D[idx, 0] = tempVal;
                for (int j = 0; j < m; j++)
                {
                    tempVal = V[j, i];
                    V[j, i] = V[j, idx];
                    V[j, idx] = tempVal;
                }
            }

            int rank = 1;
            for (int i = 0; i < m; i++)
                if (Math.Abs(D[i, 0]) > 1E-5) 
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
                    if (D[i, 0] != 0) U[j, i] = U[j, i] / D[i, 0];
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
    }
}
