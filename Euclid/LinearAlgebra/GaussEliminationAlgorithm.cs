using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Euclid.LinearAlgebra
{
    /// <summary>
    /// The elimination algorithm by Gauss (ref: https://www.mosismath.com/Matrix_Gauss/MatrixGauss.html)
    /// </summary>
    public class GaussEliminationAlgorithm
    {
        #region vars
        public int N = 4;

        public Matrix A { get; set; }
        public Vector y { get; set; }
        public Vector x { get; set; }

        private int[] Index;
        #endregion

        #region constructor
        private GaussEliminationAlgorithm(int n)
        {
            N = n;
            A = Matrix.Create(N, N);
            y = Vector.Create(N, 0);
            x = Vector.Create(N, 0);
            Index = Enumerable.Range(0, N).ToArray();
        }
        #endregion

        #region methods

        /// <summary>
        /// Create an instance of Gauss elimination algorithm
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public static GaussEliminationAlgorithm Create(int n) { return new GaussEliminationAlgorithm(n); }


        /// <summary>
        /// Switch columns since n
        /// </summary>
        /// <param name="n"></param>
        void SwitchColumns(int n)
        {
            for (int i = n; i <= N - 2; i++)
            {
                for (int j = 0; j <= N - 1; j++)
                {
                    double tempD = A[j, i];
                    A[j, i] = A[j, i + 1];
                    A[j, i + 1] = tempD;
                }

                int tempI = Index[i];
                Index[i] = Index[i + 1];
                Index[i + 1] = tempI;
            }
        }

        /// <summary>
        /// Re order
        /// </summary>
        private void Reorder()
        {
            int i;
            double[] tempVal = new double[N];

            for (i = 0; i < N; i++)
                if (Index[i] < N)
                    tempVal[Index[i]] = x[i];
            
            for (i = 0; i < N; i++) x[i] = tempVal[i];
        }

        /// <summary>
        /// Eliminate
        /// </summary>
        /// <param name="empties">Nb of empty elt</param>
        /// <returns>Calculation error</returns>
        public bool Eliminate(int empties)
        {
            bool calculationError = false;

            for (int l = 0; l < N; l++)
            {
                Index[l] = l;
                x[l] = 0.0;
                y[l] = 0.0;
            }
            for (int k = 0; k <= N - 1 - empties; k++)
            {
                int l = k + 1;
                while ((Math.Abs(A[k, k]) < 1e-8) && (l < N))
                {
                    SwitchColumns(k);
                    l++;
                }

                if (l < N)
                    for (int i = k; i <= N - 2; i++)
                        if (!calculationError)
                        
                            if (Math.Abs(A[i + 1, k]) > 1e-8)
                            {
                                for (l = k + 1; l <= N - 1; l++) A[i + 1, l] = A[i + 1, l] * A[k, k] - A[k, l] * A[i + 1, k];

                                y[i + 1] = y[i + 1] * A[k, k] - y[k] * A[i + 1, k];
                                A[i + 1, k] = 0;
                            }

                else calculationError = true;
            }

            return !calculationError;
        }

        /// <summary>
        /// Solve
        /// </summary>
        /// <param name="empties">Nb of empty elt</param>
        public void Solve(int empties)
        {
            for (int l = N - 1; l > N - empties - 1; l--) x[l] = 1.0;

            for (int k = N - empties - 1; k >= 0; k--)
            {
                for (int l = N - 1; l > k; l--) y[k] = y[k] - x[l] * A[k, l];
                x[k] = y[k] / A[k, k];
            }

            Reorder();
        }
        #endregion
    }
}
