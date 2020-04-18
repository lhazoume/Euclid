using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Euclid
{
    /// <summary>
    /// Standard polynomial
    /// </summary>
    public sealed class Polynomial
    {
        #region Declarations

        private readonly int _degree;
        private readonly double[] _terms;

        #endregion

        #region Constructors

        /// <summary>
        /// Builds a polynomial with no coefficients except the highest
        /// </summary>
        /// <param name="degree">the polynomial's degree</param>
        public Polynomial(int degree)
        {
            if (degree < 0) throw new ArgumentException("The polymial degree should be positive or null");
            _degree = degree;
            _terms = new double[1 + _degree];
            _terms[degree] = 1;
        }

        /// <summary>
        /// Builds a polynomial through the coefficients
        /// </summary>
        /// <param name="terms">the coefficients of the polynomial</param>
        public Polynomial(IList<double> terms)
        {
            if (terms == null) throw new ArgumentNullException(nameof(terms));
            if (terms.Count == 0)
            {
                _terms = new double[1];
                _terms[0] = 0;
                _degree = 0;
            }

            int termIndex = terms.Count - 1;

            while (terms[termIndex] == 0 && termIndex > 0)
                termIndex--;

            _degree = termIndex;
            _terms = new double[1 + _degree];
            for (int i = 0; i < _terms.Length; i++)
                _terms[i] = terms[i];
        }

        /// <summary>
        /// Builds a <c>Polynomial</c> from a list of coefficients
        /// </summary>
        /// <param name="coefficients">the coefficients</param>
        public Polynomial(params double[] coefficients)
            : this(coefficients.ToList())
        { }

        #endregion

        #region Methods

        /// <summary>
        /// Divides all the terms by the leading term so the polynomial has one(1) as leading term
        /// </summary>
        public void Normalize()
        {
            double normalizer = _terms.Last();
            for (int i = 0; i < _terms.Length; i++)
                _terms[i] /= normalizer;
        }

        /// <summary>
        /// Evaluates the polynomial's value for a given value
        /// </summary>
        /// <param name="x">The value for which the polynomial is evaluated</param>
        /// <returns></returns>
        public double Evaluate(double x)
        {
            double result = 0,
                tmp = 1;
            for (int i = 0; i < _terms.Length; i++)
            {
                result += tmp * _terms[i];
                tmp *= x;
            }
            return result;
        }

        /// <summary>Evaluates the polynomial's value for a given complex value</summary>
        /// <param name="x">the argument</param>
        /// <returns>a <c>Complex</c></returns>
        public Complex Evaluate(Complex x)
        {
            Complex result = Complex.Zero,
                tmp = Complex.One;
            for (int i = 0; i < _terms.Length; i++)
            {
                result += tmp * _terms[i];
                tmp *= x;
            }
            return result;
        }

        #region Complex roots

        private static double MaxValue(Polynomial p, List<Complex> z)
        {
            double buf = 0;

            for (int i = 0; i < z.Count; i++)
            {
                double n = p.Evaluate(z[i]).SquareModulus();
                if (n > buf) buf = n;
            }

            return buf;
        }

        private static Complex WeierNull(List<Complex> z, int k)
        {
            if (k < 0 || k >= z.Count) throw new ArgumentOutOfRangeException(nameof(k));

            Complex buf = Complex.One;

            for (int j = 0; j < z.Count; j++)
                if (j != k) buf *= (z[k] - z[j]);

            return buf;
        }

        /// <summary>
        /// Computes the roots of polynomial p via Weierstrass iteration.
        /// </summary>
        /// <returns>the complex roots of the <c>Polynomial</c></returns>
        public List<Tuple<Complex, int>> ComplexRoots()
        {
            double tolerance = 1e-30;
            int max_iterations = 300;

            Polynomial q = this.Clone;
            q.Normalize();
            //Polynomial q = p;

            List<Complex> z = new List<Complex>(q.Degree); // approx. for roots
            Complex[] w = new Complex[q.Degree]; // Weierstraß corrections

            // init z
            for (int k = 0; k < q.Degree; k++)
                z.Add(Complex.Exp((2 * Math.PI * k / q.Degree) * Complex.I));


            for (int iter = 0; iter < max_iterations && MaxValue(q, z) > tolerance; iter++)
                for (int i = 0; i < 10; i++)
                {
                    for (int k = 0; k < q.Degree; k++)
                        w[k] = q.Evaluate(z[k]) / WeierNull(z, k);

                    for (int k = 0; k < q.Degree; k++)
                        z[k] -= w[k];
                }

            // clean...
            for (int k = 0; k < q.Degree; k++)
            {
                z[k].Re = Math.Round(z[k].Re, 5);
                z[k].Im = Math.Round(z[k].Im, 5);
            }

            List<Tuple<Complex, int>> roots = new List<Tuple<Complex, int>>();
            while (z.Count > 0)
            {
                Complex cRef = z[0];
                Tuple<Complex, int> root = new Tuple<Complex, int>(cRef, z.Count(c => (c - cRef).SquareModulus() <= tolerance));
                z.RemoveAll(c => (c - cRef).SquareModulus() <= tolerance);
                roots.Add(root);
            }
            return roots;
        }

        #endregion

        /// <summary>
        /// Computes the real roots of the <c>Polynomial</c>
        /// </summary>
        /// <returns>a list of double</returns>
        public List<Tuple<double, int>> Roots()
        {
            List<Tuple<Complex, int>> cRoots = ComplexRoots();
            List<Tuple<double, int>> result = cRoots.FindAll(c => Math.Abs(c.Item1.Im) < 1e-8).Select(c => new Tuple<double, int>(c.Item1.Re, c.Item2)).ToList();
            result.Sort();
            return result;
        }

        /// <summary>Verifies the equality between two Polynomials</summary>
        /// <param name="other">the other Polynomial</param>
        /// <returns><c>true</c> if they are equal, <c>false</c> otherwise</returns>
        public bool Equals(Polynomial other)
        {
            // Reject equality when the argument is null or has a different shape.
            if (other == null)
                return false;
            if (_degree != other._degree)
                return false;

            // Accept if the argument is the same object as this.
            if (ReferenceEquals(this, other))
                return true;

            // If all else fails, perform elementwise comparison.
            for (int i = 0; i < _terms.Length; i++)
                if (_terms[i] != other._terms[i])
                    return false;

            return true;
        }
        #endregion

        #region Accessors

        /// <summary>
        /// Accesses the terms of the polynomial
        /// </summary>
        /// <param name="i">the index</param>
        /// <returns>a term of the polynomial</returns>
        public double this[int i]
        {
            get { return i < _terms.Length ? _terms[i] : 0; }
        }

        /// <summary>
        /// Returns the polynomial's degree
        /// </summary>
        public int Degree
        {
            get { return _degree; }
        }

        /// <summary>
        /// Returns the derivative polynomial (one degree less)
        /// </summary>
        public Polynomial Derivative
        {
            get
            {
                if (_terms.Length <= 1)
                    return new Polynomial(0);
                List<double> newTerms = new List<double>();
                for (int i = 1; i < _terms.Length; i++)
                    newTerms.Add(_terms[i] * i);
                return new Polynomial(newTerms);
            }

        }

        /// <summary>Returns a string representing the polynomial</summary>
        /// <returns>a <c>string</c></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(_terms[0].ToString());
            for (int i = 1; i <= _degree; i++)
            {
                double t = _terms[i];
                if (t != 0)
                {
                    if (t > 0) sb.Append("+");
                    sb.Append(t == 1 ? "" : t.ToString());
                    sb.Append("x");
                    if (i > 1) sb.Append("^" + i);
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Returns a string representing the polynomial
        /// </summary>
        /// <param name="format">the format for all coefficients</param>
        /// <returns>a <c>String</c></returns>
        public string ToString(string format)
        {
            StringBuilder sb = new StringBuilder(_terms[0].ToString(format));
            for (int i = 1; i <= _degree; i++)
            {
                double t = _terms[i];
                if (t != 0)
                {
                    if (t > 0) sb.Append("+");
                    sb.Append(t == 1 ? "" : t.ToString(format));
                    sb.Append("x");
                    if (i > 1) sb.Append("^" + i);
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Returns a deep copy of the polynomial
        /// </summary>
        public Polynomial Clone
        {
            get { return new Polynomial(_terms); }
        }

        #endregion

        #region Operators

        #region Multiplications
        /// <summary>Multiplies a polynomial by a scalar</summary>
        /// <param name="p">the <c>Polynomial</c></param>
        /// <param name="f">the scalar</param>
        /// <returns>a <c>Polynomial</c></returns>
        public static Polynomial operator *(Polynomial p, double f)
        {
            if (p == null) throw new ArgumentNullException(nameof(p));
            Polynomial tmp = p.Clone;
            for (int i = 0; i < p._terms.Length; i++)
                tmp._terms[i] *= f;
            return tmp;
        }

        /// <summary>Multiplies a polynomial by a scalar</summary>
        /// <param name="f">the scalar</param>
        /// <param name="p">the <c>Polynomial</c></param>
        /// <returns>a <c>Polynomial</c></returns>
        public static Polynomial operator *(double f, Polynomial p)
        {
            return p * f;
        }

        /// <summary> multiplies two polynomials </summary>
        /// <param name="p1">the left hand side</param>
        /// <param name="p2">the right hand side</param>
        /// <returns>the <c>Polynomial</c> result of the polynomial</returns>
        public static Polynomial operator *(Polynomial p1, Polynomial p2)
        {
            if (p1 == null) throw new ArgumentNullException(nameof(p1));
            if (p2 == null) throw new ArgumentNullException(nameof(p2));
            Polynomial r = new Polynomial(0.0);

            for (int i = 0; i <= p1.Degree; i++)
                for (int j = 0; j <= p2.Degree; j++)
                    r += (p1[i] * p2[j]) * new Polynomial(i + j);

            return r;
        }

        /// <summary>Raises a Polynomial to an integer power</summary>
        /// <param name="p">the Polynomial</param>
        /// <param name="n">the power</param>
        /// <returns>a <c>Polynomial</c></returns>
        public static Polynomial operator ^(Polynomial p, int n)
        {
            return Polynomial.Power(p, n);
        }

        #endregion

        #region Additions / substractions

        /// <summary>
        /// Performs a polynomial addition
        /// </summary>
        /// <param name="p1">First matrix</param>
        /// <param name="p2">Second matrix</param>
        /// <returns>The sum of m1 and m2</returns>
        private static Polynomial Add(Polynomial p1, Polynomial p2)
        {
            if (p1 == null) throw new ArgumentNullException(nameof(p1));
            if (p2 == null) throw new ArgumentNullException(nameof(p2));
            double[] newTerms = new double[Math.Max(p1._terms.Length, p2._terms.Length)];

            for (int i = 0; i < newTerms.Length; i++)
                newTerms[i] = (i < p1._terms.Length ? p1._terms[i] : 0) + (i < p2._terms.Length ? p2._terms[i] : 0);

            return new Polynomial(newTerms);
        }

        /// <summary>
        /// Adds a polynomial to a scalar
        /// </summary>
        /// <param name="p">the polynomial left hand side</param>
        /// <param name="c">the scalar right hand side</param>
        /// <returns>the <c>Polynomial</c> result of the adition</returns>
        public static Polynomial operator +(Polynomial p, double c)
        {
            if (p == null) throw new ArgumentNullException(nameof(p));
            Polynomial tmp = p.Clone;
            p._terms[0] += c;
            return tmp;
        }

        /// <summary>
        /// Adds a polynomial to a scalar
        /// </summary>
        /// <param name="c">the scalar left hand side</param>
        /// <param name="p">the polynomial right hand side</param>
        /// <returns>the <c>Polynomial</c> result of the addition</returns>
        public static Polynomial operator +(double c, Polynomial p)
        {
            return p + c;
        }

        /// <summary>Substracts a scalar to a <c>Polynomial</c></summary>
        /// <param name="p">the <c>Polynomial</c></param>
        /// <param name="c">the scalar</param>
        /// <returns>a <c>Polynomial</c></returns>
        public static Polynomial operator -(Polynomial p, double c)
        {
            return p + (-c);
        }

        /// <summary>Substracts a <c>Polynomial to a scalar</c></summary>
        /// <param name="c">the scalar left hand side</param>
        /// <param name="p">the <c>Polynomial</c> right hand side</param>
        /// <returns>a <c>Polynomial</c></returns>
        public static Polynomial operator -(double c, Polynomial p)
        {
            return (-1) * p + c;
        }

        /// <summary>Returns the opposite of the <c>Polynomial</c></summary>
        /// <param name="p">the <c>Polynomial</c></param>
        /// <returns>a <c>Polynomial</c></returns>
        public static Polynomial operator -(Polynomial p)
        {
            return p * -1;
        }

        /// <summary>
        /// Adds two polynomials
        /// </summary>
        /// <param name="p1">the left hand side</param>
        /// <param name="p2">the right hand side</param>
        /// <returns>the <c>Polynomial</c> result of the addition</returns>
        public static Polynomial operator +(Polynomial p1, Polynomial p2)
        {
            return Polynomial.Add(p1, p2);
        }

        /// <summary>
        /// Substracts one polynomial to another
        /// </summary>
        /// <param name="p1">the left hand side</param>
        /// <param name="p2">the right hand side</param>
        /// <returns>the <c>Polynomial</c> result of the substraction </returns>
        public static Polynomial operator -(Polynomial p1, Polynomial p2)
        {
            return Polynomial.Add(p1, -p2);
        }

        #endregion

        /// <summary>
        /// Evaluates the Polynomial raised to a power specified by pow.
        /// </summary>
        /// <param name="p">the <c>Polynomial</c> target</param>
        /// <param name="pow">The power we want to raise the Polynomial to</param>
        /// <returns>The Polynomial, raised to the power pow</returns>
        public static Polynomial Power(Polynomial p, int pow)
        {
            if (p == null) throw new ArgumentNullException(nameof(p));
            if (pow == 0) return new Polynomial(new double[] { 1 });
            if (pow == 1) return p.Clone;
            if (pow < 0) throw new ArgumentException("A polynomial can not be raised to a negative power");

            Polynomial tmp = p.Clone;

            for (int i = 2; i <= pow; i++)
                tmp *= p;
            return tmp;
        }

        #endregion
    }
}
