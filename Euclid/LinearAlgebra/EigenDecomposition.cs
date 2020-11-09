using System;
using System.Collections.Generic;

namespace Euclid.LinearAlgebra
{
    /// <summary>Class used to diagonalize symmetric matrices</summary>
    public class EigenDecomposition
    {
        #region Declarations
        private readonly double[] _d;
        private readonly double[] _e;
        private readonly Matrix _V;
        private Matrix _H;
        #endregion

        #region Constructors
        /// <summary>Initiates the class</summary>
        /// <param name="matrix">the <c>Matrix</c> to decompose</param>
        public EigenDecomposition(Matrix matrix)
        {
            if (matrix == null) throw new ArgumentNullException(nameof(matrix));

            int n = matrix.Columns;
            _V = matrix.Clone;
            _d = new double[n];
            _e = new double[n];
            Solve();
        }
        #endregion

        #region Accessors
        /// <summary>Returns the <c>Complex</c> eigen values of the matrix</summary>
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

        /// <summary>Returns the real eigen values of the matrix</summary>
        public double[] RealEigenValues
        {
            get
            {
                int n = _V.Rows;
                List<double> eigenValues = new List<double>();
                for (int i = 0; i < n; i++)
                    if (_e[i] == 0)
                        eigenValues.Add(_d[i]);
                return eigenValues.ToArray();
            }
        }

        /// <summary>Returns the diagonal <c>Matrix</c> which is the diagonalized form of the initial matrix</summary>
        public Matrix DiagonalMatrix
        {
            get
            {
                int n = _V.Rows;
                Matrix D = Matrix.CreateSquare(n);
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

        /// <summary>Returns an array of all the eigen vectors of the matrix</summary>
        public Vector[] EigenVectors
        {
            get
            {
                Vector[] result = new Vector[_V.Columns];
                for (int i = 0; i < _V.Columns; i++)
                    result[i] = _V.Column(i);
                return result;
            }
        }

        /// <summary>Returns an array of the eigen vectors attached to the real eigen values of the matrix</summary>
        public Vector[] RealEigenVectors
        {
            get
            {
                List<Vector> result = new List<Vector>();
                for (int i = 0; i < _V.Columns; i++)
                    if (_e[i] == 0)
                        result.Add(_V.Column(i));
                return result.ToArray();
            }
        }

        /// <summary>Returns an array of pairs of eigen values and eigen vectors</summary>
        public Tuple<double, Vector>[] RealEigenPairs
        {
            get
            {
                List<Tuple<double, Vector>> result = new List<Tuple<double, Vector>>();
                for (int i = 0; i < _V.Columns; i++)
                    if (_e[i] == 0)
                        result.Add(new Tuple<double, Vector>(_d[i], _V.Column(i)));
                return result.ToArray();
            }
        }
        #endregion

        #region Methods
        /// <summary>Tridiagonalizes the matrix and then diagonalize it in the complex space</summary>
        private void Solve()
        {
            if (_V.IsSymetric)
            {
                SymmetricTridiagonalize();
                SymmetricDiagonalize();
            }
            else
            {
                _H = _V.Clone;
                NonsymmetricReduceToHessenberg();
                NonsymmetricReduceHessenberToRealSchur();
            }
        }

        /// <summary>Symmetric Householder reduction to tridiagonal form.</summary>
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
                    scale += Math.Abs(_d[k]);

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
                    h -= f * g;
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

        /// <summary>Symmetric tridiagonal QL algorithm</summary>
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
                        iter += 1; // (Could check iteration count here.)

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

                        f += h;

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

        /// <summary>Nonsymmetric reduction to Hessenberg form</summary>
        private void NonsymmetricReduceToHessenberg()
        {
            // This is derived from the Algol procedures orthes and ortran,
            // by Martin and Wilkinson, Handbook for Auto. Comp.,
            // Vol.ii-Linear Algebra, and the corresponding
            // Fortran subroutines in EISPACK.

            int low = 0;
            int high = _H.Rows - 1;

            double[] ort = new double[_H.Rows];

            for (int m = low + 1; m <= high - 1; m++)
            {

                // Scale column.
                double scale = 0.0;
                for (int i = m; i <= high; i++)
                    scale += Math.Abs(_H[i, m - 1]);

                if (scale != 0.0)
                {

                    // Compute Householder transformation.

                    double h = 0.0;
                    for (int i = high; i >= m; i--)
                    {
                        ort[i] = _H[i, m - 1] / scale;
                        h += ort[i] * ort[i];
                    }

                    double g = Math.Sqrt(h);
                    if (ort[m] > 0)
                        g = -g;

                    h -= ort[m] * g;
                    ort[m] = ort[m] - g;

                    // Apply Householder similarity transformation
                    // H = (I-u*u'/h)*H*(I-u*u')/h)

                    for (int j = m; j < _H.Rows; j++)
                    {
                        double f = 0.0;
                        for (int i = high; i >= m; i--)
                            f += ort[i] * _H[i, j];

                        f /= h;
                        for (int i = m; i <= high; i++)
                            _H[i, j] -= f * ort[i];
                    }

                    for (int i = 0; i <= high; i++)
                    {
                        double f = 0.0;
                        for (int j = high; j >= m; j--)
                            f += ort[j] * _H[i, j];

                        f /= h;
                        for (int j = m; j <= high; j++)
                            _H[i, j] -= f * ort[j];
                    }

                    ort[m] = scale * ort[m];
                    _H[m, m - 1] = scale * g;
                }
            }

            // Accumulate transformations (Algol's ortran).

            for (int i = 0; i < _H.Rows; i++)
                for (int j = 0; j < _H.Rows; j++)
                    _V[i, j] = (i == j ? 1.0 : 0.0);

            for (int m = high - 1; m >= low + 1; m--)
            {
                if (_H[m, m - 1] != 0.0)
                {
                    for (int i = m + 1; i <= high; i++)
                        ort[i] = _H[i, m - 1];

                    for (int j = m; j <= high; j++)
                    {
                        double g = 0.0;
                        for (int i = m; i <= high; i++)
                            g += ort[i] * _V[i, j];

                        // Double division avoids possible underflow
                        g = (g / ort[m]) / _H[m, m - 1];
                        for (int i = m; i <= high; i++)
                            _V[i, j] += g * ort[i];
                    }
                }
            }
        }

        /// <summary>Nonsymmetric reduction from Hessenberg to real Schur form</summary>
        private void NonsymmetricReduceHessenberToRealSchur()
        {
            // This is derived from the Algol procedure hqr2,
            // by Martin and Wilkinson, Handbook for Auto. Comp.,
            // Vol.ii-Linear Algebra, and the corresponding
            // Fortran subroutine in EISPACK.

            // Initialize

            int nn = _H.Rows; ;
            int n = nn - 1;
            int low = 0;
            int high = nn - 1;
            double eps = 1e-12;
            double exshift = 0.0;
            double p = 0, q = 0, r = 0, s = 0, z = 0, t, w, x, y;

            // Store roots isolated by balanc and compute matrix norm

            double norm = 0.0;
            for (int i = 0; i < nn; i++)
            {
                if (i < low | i > high)
                {
                    _d[i] = _H[i, i];
                    _e[i] = 0.0;
                }

                for (int j = Math.Max(i - 1, 0); j < nn; j++)
                    norm += Math.Abs(_H[i, j]);
            }

            // Outer loop over eigenvalue index

            int iter = 0;
            while (n >= low)
            {

                // Look for single small sub-diagonal element

                int l = n;
                while (l > low)
                {
                    s = Math.Abs(_H[l - 1, l - 1]) + Math.Abs(_H[l, l]);

                    if (s == 0.0)
                        s = norm;

                    if (Math.Abs(_H[l, l - 1]) < eps * s)
                        break;

                    l--;
                }

                // Check for convergence
                // One root found

                if (l == n)
                {
                    _H[n, n] = _H[n, n] + exshift;
                    _d[n] = _H[n, n];
                    _e[n] = 0.0;
                    n--;
                    iter = 0;

                    // Two roots found
                }
                else if (l == n - 1)
                {
                    w = _H[n, n - 1] * _H[n - 1, n];
                    p = (_H[n - 1, n - 1] - _H[n, n]) / 2.0;
                    q = p * p + w;
                    z = Math.Sqrt(Math.Abs(q));
                    _H[n, n] = _H[n, n] + exshift;
                    _H[n - 1, n - 1] = _H[n - 1, n - 1] + exshift;
                    x = _H[n, n];

                    // Real pair

                    if (q >= 0)
                    {
                        if (p >= 0)
                            z = p + z;
                        else
                            z = p - z;

                        _d[n - 1] = x + z;
                        _d[n] = _d[n - 1];
                        if (z != 0.0)
                            _d[n] = x - w / z;

                        _e[n - 1] = 0.0;
                        _e[n] = 0.0;
                        x = _H[n, n - 1];
                        s = Math.Abs(x) + Math.Abs(z);
                        p = x / s;
                        q = z / s;
                        r = Math.Sqrt(p * p + q * q);
                        p /= r;
                        q /= r;

                        // Row modification

                        for (int j = n - 1; j < nn; j++)
                        {
                            z = _H[n - 1, j];
                            _H[n - 1, j] = q * z + p * _H[n, j];
                            _H[n, j] = q * _H[n, j] - p * z;
                        }

                        // Column modification

                        for (int i = 0; i <= n; i++)
                        {
                            z = _H[i, n - 1];
                            _H[i, n - 1] = q * z + p * _H[i, n];
                            _H[i, n] = q * _H[i, n] - p * z;
                        }

                        // Accumulate transformations

                        for (int i = low; i <= high; i++)
                        {
                            z = _V[i, n - 1];
                            _V[i, n - 1] = q * z + p * _V[i, n];
                            _V[i, n] = q * _V[i, n] - p * z;
                        }

                        // Complex pair
                    }
                    else
                    {
                        _d[n - 1] = x + p;
                        _d[n] = x + p;
                        _e[n - 1] = z;
                        _e[n] = -z;
                    }

                    n -= 2;
                    iter = 0;

                    // No convergence yet
                }
                else
                {

                    // Form shift

                    x = _H[n, n];
                    y = 0.0;
                    w = 0.0;
                    if (l < n)
                    {
                        y = _H[n - 1, n - 1];
                        w = _H[n, n - 1] * _H[n - 1, n];
                    }

                    // Wilkinson's original ad hoc shift

                    if (iter == 10)
                    {
                        exshift += x;
                        for (int i = low; i <= n; i++)
                            _H[i, i] -= x;

                        s = Math.Abs(_H[n, n - 1]) + Math.Abs(_H[n - 1, n - 2]);
                        x = y = 0.75 * s;
                        w = (-0.4375) * s * s;
                    }

                    // MATLAB's new ad hoc shift

                    if (iter == 30)
                    {
                        s = (y - x) / 2.0;
                        s = s * s + w;
                        if (s > 0)
                        {
                            s = Math.Sqrt(s);
                            if (y < x)
                                s = -s;

                            s = x - w / ((y - x) / 2.0 + s);
                            for (int i = low; i <= n; i++)
                                _H[i, i] -= s;

                            exshift += s;
                            x = y = w = 0.964;
                        }
                    }

                    iter += 1; // (Could check iteration count here.)

                    // Look for two consecutive small sub-diagonal elements

                    int m = n - 2;
                    while (m >= l)
                    {
                        z = _H[m, m];
                        r = x - z;
                        s = y - z;
                        p = (r * s - w) / _H[m + 1, m] + _H[m, m + 1];
                        q = _H[m + 1, m + 1] - z - r - s;
                        r = _H[m + 2, m + 1];
                        s = Math.Abs(p) + Math.Abs(q) + Math.Abs(r);
                        p /= s;
                        q /= s;
                        r /= s;

                        if (m == l)
                            break;

                        if (Math.Abs(_H[m, m - 1]) * (Math.Abs(q) + Math.Abs(r)) < eps * (Math.Abs(p) * (Math.Abs(_H[m - 1, m - 1]) + Math.Abs(z) + Math.Abs(_H[m + 1, m + 1]))))
                            break;

                        m--;
                    }

                    for (int i = m + 2; i <= n; i++)
                    {
                        _H[i, i - 2] = 0.0;
                        if (i > m + 2)
                            _H[i, i - 3] = 0.0;
                    }

                    // Double QR step involving rows l:n and columns m:n

                    for (int k = m; k <= n - 1; k++)
                    {
                        bool notlast = (k != n - 1);

                        if (k != m)
                        {
                            p = _H[k, k - 1];
                            q = _H[k + 1, k - 1];
                            r = (notlast ? _H[k + 2, k - 1] : 0.0);
                            x = Math.Abs(p) + Math.Abs(q) + Math.Abs(r);
                            if (x != 0.0)
                            {
                                p /= x;
                                q /= x;
                                r /= x;
                            }
                        }

                        if (x == 0.0)
                            break;

                        s = Math.Sqrt(p * p + q * q + r * r);
                        if (p < 0)
                            s = -s;

                        if (s != 0.0)
                        {
                            if (k != m)
                                _H[k, k - 1] = (-s) * x;
                            else if (l != m)
                                _H[k, k - 1] = -_H[k, k - 1];

                            p += s;
                            x = p / s;
                            y = q / s;
                            z = r / s;
                            q /= p;
                            r /= p;

                            // Row modification

                            for (int j = k; j < nn; j++)
                            {
                                p = _H[k, j] + q * _H[k + 1, j];

                                if (notlast)
                                {
                                    p += r * _H[k + 2, j];
                                    _H[k + 2, j] = _H[k + 2, j] - p * z;
                                }

                                _H[k, j] = _H[k, j] - p * x;
                                _H[k + 1, j] = _H[k + 1, j] - p * y;
                            }

                            // Column modification

                            for (int i = 0; i <= Math.Min(n, k + 3); i++)
                            {

                                p = x * _H[i, k] + y * _H[i, k + 1];

                                if (notlast)
                                {
                                    p += z * _H[i, k + 2];
                                    _H[i, k + 2] = _H[i, k + 2] - p * r;
                                }

                                _H[i, k] = _H[i, k] - p;
                                _H[i, k + 1] = _H[i, k + 1] - p * q;
                            }

                            // Accumulate transformations

                            for (int i = low; i <= high; i++)
                            {
                                p = x * _V[i, k] + y * _V[i, k + 1];

                                if (notlast)
                                {
                                    p += z * _V[i, k + 2];
                                    _V[i, k + 2] = _V[i, k + 2] - p * r;
                                }

                                _V[i, k] = _V[i, k] - p;
                                _V[i, k + 1] = _V[i, k + 1] - p * q;
                            }
                        } // (s != 0)
                    } // k loop
                } // check convergence
            } // while (n >= low)

            // Backsubstitute to find vectors of upper triangular form

            if (norm == 0.0)
                return;

            for (n = nn - 1; n >= 0; n--)
            {
                p = _d[n];
                q = _e[n];

                // Real vector

                if (q == 0.0)
                {
                    int l = n;
                    _H[n, n] = 1.0;
                    for (int i = n - 1; i >= 0; i--)
                    {
                        w = _H[i, i] - p;
                        r = 0.0;
                        for (int j = l; j <= n; j++)
                            r += _H[i, j] * _H[j, n];

                        if (_e[i] < 0.0)
                        {
                            z = w;
                            s = r;
                        }
                        else
                        {
                            l = i;
                            if (_e[i] == 0.0)
                            {
                                _H[i, n] = w != 0.0 ? (-r) / w : (-r) / (eps * norm);

                                // Solve real equations
                            }
                            else
                            {
                                x = _H[i, i + 1];
                                y = _H[i + 1, i];
                                q = (_d[i] - p) * (_d[i] - p) + _e[i] * _e[i];
                                t = (x * s - z * r) / q;
                                _H[i, n] = t;
                                _H[i + 1, n] = Math.Abs(x) > Math.Abs(z) ? (-r - w * t) / x : (-s - y * t) / z;
                            }

                            // Overflow control

                            t = Math.Abs(_H[i, n]);
                            if ((eps * t) * t > 1)
                                for (int j = i; j <= n; j++)
                                    _H[j, n] = _H[j, n] / t;
                        }
                    }

                    // Complex vector
                }
                else if (q < 0)
                {
                    int l = n - 1;

                    // Last vector component imaginary so matrix is triangular

                    if (Math.Abs(_H[n, n - 1]) > Math.Abs(_H[n - 1, n]))
                    {
                        _H[n - 1, n - 1] = q / _H[n, n - 1];
                        _H[n - 1, n] = (-(_H[n, n] - p)) / _H[n, n - 1];
                    }
                    else
                    {
                        Complex a = Complex.I * (-_H[n - 1, n]),
                            b = new Complex(_H[n - 1, n - 1] - p, q),
                            c = a / b;
                        _H[n - 1, n - 1] = c.Re;
                        _H[n - 1, n] = c.Im;
                    }

                    _H[n, n - 1] = 0.0;
                    _H[n, n] = 1.0;
                    for (int i = n - 2; i >= 0; i--)
                    {
                        double ra, sa, vr, vi;
                        ra = 0.0;
                        sa = 0.0;
                        for (int j = l; j <= n; j++)
                        {
                            ra += _H[i, j] * _H[j, n - 1];
                            sa += _H[i, j] * _H[j, n];
                        }

                        w = _H[i, i] - p;

                        if (_e[i] < 0.0)
                        {
                            z = w;
                            r = ra;
                            s = sa;
                        }
                        else
                        {
                            l = i;
                            if (_e[i] == 0.0)
                            {
                                Complex a = new Complex(-ra, -sa),
                                    b = new Complex(w, q),
                                    c = a / b;
                                _H[i, n - 1] = c.Re;
                                _H[i, n] = c.Im;
                            }
                            else
                            {

                                // Solve complex equations

                                x = _H[i, i + 1];
                                y = _H[i + 1, i];

                                vr = (_d[i] - p) * (_d[i] - p) + _e[i] * _e[i] - q * q;
                                vi = (_d[i] - p) * 2.0 * q;
                                if ((vr == 0.0) && (vi == 0.0))
                                    vr = eps * norm * (Math.Abs(w) + Math.Abs(q) + Math.Abs(x) + Math.Abs(y) + Math.Abs(z));

                                Complex a = new Complex(x * r - z * ra + q * sa, x * s - z * sa - q * ra),
                                    b = new Complex(vr, vi),
                                    c = a / b;
                                _H[i, n - 1] = c.Re;
                                _H[i, n] = c.Im;
                                if (Math.Abs(x) > (Math.Abs(z) + Math.Abs(q)))
                                {
                                    _H[i + 1, n - 1] = (-ra - w * _H[i, n - 1] + q * _H[i, n]) / x;
                                    _H[i + 1, n] = (-sa - w * _H[i, n] - q * _H[i, n - 1]) / x;
                                }
                                else
                                {
                                    Complex a1 = new Complex(-r - y * _H[i, n - 1], -s - y * _H[i, n]),
                                        b1 = new Complex(z, q),
                                        c1 = a1 / b1;
                                    _H[i + 1, n - 1] = c1.Re;
                                    _H[i + 1, n] = c1.Im;
                                }
                            }

                            // Overflow control

                            t = Math.Max(Math.Abs(_H[i, n - 1]), Math.Abs(_H[i, n]));
                            if ((eps * t) * t > 1)
                                for (int j = i; j <= n; j++)
                                {
                                    _H[j, n - 1] = _H[j, n - 1] / t;
                                    _H[j, n] = _H[j, n] / t;
                                }
                        }
                    }
                }
            }

            // Vectors of isolated roots

            for (int i = 0; i < nn; i++)
                if (i < low | i > high)
                    for (int j = i; j < nn; j++)
                        _V[i, j] = _H[i, j];

            // Back transformation to get eigenvectors of original matrix

            for (int j = nn - 1; j >= low; j--)
                for (int i = low; i <= high; i++)
                {
                    z = 0.0;
                    for (int k = low; k <= Math.Min(j, high); k++)
                        z += _V[i, k] * _H[k, j];

                    _V[i, j] = z;
                }
        }
        #endregion
    }
}
