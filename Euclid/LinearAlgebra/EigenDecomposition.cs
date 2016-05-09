using System;

namespace Euclid.LinearAlgebra
{
    public class EigenDecomposition
    {
        #region Declarations
        private double[] _d, _e;      /// <summary>Arrays for internal storage of eigenvalues.</summary>
        private Matrix _V;          /// <summary>Array for internal storage of eigenvectors.</summary>
        #endregion

        #region Constructors
        /// <summary>
        /// Check for symmetry, then construct the eigenvalue decomposition
        /// </summary>
        /// <remarks>Provides access to D and V</remarks>
        /// <param name="Arg">Square matrix</param>
        public EigenDecomposition(Matrix matrix)
        {
            if (!matrix.IsSymetric)
                throw new ArgumentException("the argument matrix should be symmetric");

            int n = matrix.Columns;
            _V = matrix.Clone;
            _d = new double[n];
            _e = new double[n];

            SymmetricTridiagonalize();
            SymmetricDiagonalize();
        }
        #endregion

        #region Accessors

        public Complex[] EigenValues
        {
            get
            {
                int n = _V.Rows;
                Complex[] eigenValues = new Complex[n];
                for (int i = 0; i < eigenValues.Length; i++)
                    eigenValues[i] = new Complex(_d[i], _e[i]);
                return eigenValues;
            }
        }

        public Matrix DiagonalMatrix
        {
            get
            {
                int n = _V.Rows;
                Matrix D = new Matrix(n);
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                        D[i, j] = 0.0;

                    D[i, i] = _d[i];

                    if (_e[i] > 0)
                        D[i, i + 1] = _e[i];
                    else if (_e[i] < 0)
                        D[i, i - 1] = _e[i];
                }

                return D;
            }
        }

        public Matrix EigenVectors
        {
            get { return _V.Clone; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Symmetric Householder reduction to tridiagonal form.
        /// </summary>
        private void SymmetricTridiagonalize()
        {
            // This is derived from the Algol procedures tred2 by
            // Bowdler, Martin, Reinsch, and Wilkinson, Handbook for
            // Auto. Comp., Vol.ii-Linear Algebra, and the corresponding
            // Fortran subroutine in EISPACK.
            int n = _V.Rows;
            for (int j = 0; j < n; j++)
                _d[j] = _V[n - 1, j];

            // Householder reduction to tridiagonal form.

            for (int i = n - 1; i > 0; i--)
            {
                // Scale to avoid under/overflow.

                double scale = 0.0,
                    h = 0.0;

                for (int k = 0; k < i; k++)
                    scale = scale + Math.Abs(_d[k]);

                if (scale == 0.0)
                {
                    _e[i] = _d[i - 1];
                    for (int j = 0; j < i; j++)
                    {
                        _d[j] = _V[i - 1, j];
                        _V[i, j] = 0.0;
                        _V[j, i] = 0.0;
                    }
                }
                else
                {
                    // Generate Householder vector.

                    for (int k = 0; k < i; k++)
                    {
                        _d[k] /= scale;
                        h += _d[k] * _d[k];
                    }

                    double f = _d[i - 1],
                        g = Math.Sqrt(h);
                    if (f > 0)
                        g = -g;

                    _e[i] = scale * g;
                    h = h - f * g;
                    _d[i - 1] = f - g;
                    for (int j = 0; j < i; j++)
                        _e[j] = 0.0;

                    // Apply similarity transformation to remaining columns.

                    for (int j = 0; j < i; j++)
                    {
                        f = _d[j];
                        _V[j, i] = f;
                        g = _e[j] + _V[j, j] * f;

                        for (int k = j + 1; k <= i - 1; k++)
                        {
                            g += _V[k, j] * _d[k];
                            _e[k] += _V[k, j] * f;
                        }

                        _e[j] = g;
                    }

                    f = 0.0;

                    for (int j = 0; j < i; j++)
                    {
                        _e[j] /= h;
                        f += _e[j] * _d[j];
                    }

                    double hh = f / (h + h);

                    for (int j = 0; j < i; j++)
                        _e[j] -= hh * _d[j];

                    for (int j = 0; j < i; j++)
                    {
                        f = _d[j];
                        g = _e[j];

                        for (int k = j; k <= i - 1; k++)
                            _V[k, j] -= (f * _e[k] + g * _d[k]);

                        _d[j] = _V[i - 1, j];
                        _V[i, j] = 0.0;
                    }
                }

                _d[i] = h;
            }

            // Accumulate transformations.

            for (int i = 0; i < n - 1; i++)
            {
                _V[n - 1, i] = _V[i, i];
                _V[i, i] = 1.0;
                double h = _d[i + 1];
                if (h != 0.0)
                {
                    for (int k = 0; k <= i; k++)
                        _d[k] = _V[k, i + 1] / h;

                    for (int j = 0; j <= i; j++)
                    {
                        double g = 0.0;
                        for (int k = 0; k <= i; k++)
                            g += _V[k, i + 1] * _V[k, j];

                        for (int k = 0; k <= i; k++)
                            _V[k, j] -= g * _d[k];
                    }
                }

                for (int k = 0; k <= i; k++)
                    _V[k, i + 1] = 0.0;
            }

            for (int j = 0; j < n; j++)
            {
                _d[j] = _V[n - 1, j];
                _V[n - 1, j] = 0.0;
            }

            _V[n - 1, n - 1] = 1.0;
            _e[0] = 0.0;
        }

        /// <summary>
        /// Symmetric tridiagonal QL algorithm.
        /// </summary>
        private void SymmetricDiagonalize()
        {
            // This is derived from the Algol procedures tql2, by
            // Bowdler, Martin, Reinsch, and Wilkinson, Handbook for
            // Auto. Comp., Vol.ii-Linear Algebra, and the corresponding
            // Fortran subroutine in EISPACK.
            int n = _V.Rows;
            for (int i = 1; i < n; i++)
                _e[i - 1] = _e[i];

            _e[n - 1] = 0.0;

            double f = 0.0,
                tst1 = 0.0,
                eps = 1e-12;
            for (int l = 0; l < n; l++)
            {
                // Find small subdiagonal element

                tst1 = Math.Max(tst1, Math.Abs(_d[l]) + Math.Abs(_e[l]));
                int m = l;
                while (m < n)
                {
                    if (Math.Abs(_e[m]) <= eps * tst1)
                        break;
                    m++;
                }

                // If m == l, d[l] is an eigenvalue,
                // otherwise, iterate.

                if (m > l)
                {
                    int iter = 0;
                    do
                    {
                        iter = iter + 1; // (Could check iteration count here.)

                        // Compute implicit shift

                        double g = _d[l],
                            p = (_d[l + 1] - g) / (2.0 * _e[l]),
                            r = Math.Sqrt(1.0 + p * p);
                        if (p < 0)
                            r = -r;

                        _d[l] = _e[l] / (p + r);
                        _d[l + 1] = _e[l] * (p + r);

                        double dl1 = _d[l + 1],
                            h = g - _d[l];
                        for (int i = l + 2; i < n; i++)
                            _d[i] -= h;

                        f = f + h;

                        // Implicit QL transformation.

                        p = _d[m];
                        double c = 1.0, c2 = c, c3 = c,
                            el1 = _e[l + 1],
                            s = 0.0,
                            s2 = 0.0;
                        for (int i = m - 1; i >= l; i--)
                        {
                            c3 = c2;
                            c2 = c;
                            s2 = s;
                            g = c * _e[i];
                            h = c * p;
                            r = Math.Sqrt(p * p + _e[i] * _e[i]);
                            _e[i + 1] = s * r;
                            s = _e[i] / r;
                            c = p / r;
                            p = c * _d[i] - s * g;
                            _d[i + 1] = h + s * (c * g + s * _d[i]);

                            // Accumulate transformation.

                            for (int k = 0; k < n; k++)
                            {
                                h = _V[k, i + 1];
                                _V[k, i + 1] = s * _V[k, i] + c * h;
                                _V[k, i] = c * _V[k, i] - s * h;
                            }
                        }

                        p = (-s) * s2 * c3 * el1 * _e[l] / dl1;
                        _e[l] = s * p;
                        _d[l] = c * p;

                        // Check for convergence.
                    }
                    while (Math.Abs(_e[l]) > eps * tst1);
                }

                _d[l] = _d[l] + f;
                _e[l] = 0.0;
            }

            // Sort eigenvalues and corresponding vectors.

            for (int i = 0; i < n - 1; i++)
            {
                int k = i;
                double p = _d[i];
                for (int j = i + 1; j < n; j++)
                    if (_d[j] < p)
                    {
                        k = j;
                        p = _d[j];
                    }

                if (k != i)
                {
                    _d[k] = _d[i];
                    _d[i] = p;
                    for (int j = 0; j < n; j++)
                    {
                        p = _V[j, i];
                        _V[j, i] = _V[j, k];
                        _V[j, k] = p;
                    }
                }
            }
        }

        #endregion
    }
}
