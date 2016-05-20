using System;

namespace Euclid
{
    /// <summary>
    /// Standard Complex numbers
    /// </summary>
    public sealed class Complex
    {
        #region Declarations
        private double _re, _im;
        #endregion

        #region Constructors
        /// <summary>
        /// Builds a <c>Complex</c> number
        /// </summary>
        /// <param name="real">the real part of the <c>Complex</c></param>
        /// <param name="imaginary">the imaginary part of the <c>Complex</c></param>
        public Complex(double real, double imaginary)
        {
            _re = real;
            _im = imaginary;
        }

        /// <summary>
        /// Builds a <c>Complex</c> number with its real and imaginary parts at zero
        /// </summary>
        public Complex()
            : this(0, 0)
        { }
        #endregion

        #region Accessors
        /// <summary>
        /// Returns the real part of the <c>Complex</c>
        /// </summary>
        public double Re
        {
            get { return _re; }
            set { _re = value; }
        }

        /// <summary>
        /// Returns the imaginary part of the <c>Complex</c>
        /// </summary>
        public double Im
        {
            get { return _im; }
            set { _im = value; }
        }

        /// <summary>
        /// The conjugate number of the <c>Complex</c>
        /// </summary>
        public Complex Conjugate
        {
            get { return new Complex(_re, -_im); }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Returns the modulus of the <c>Complex</c>
        /// </summary>
        /// <returns>the modulus of the complex</returns>
        public double Modulus()
        {
            return Math.Sqrt(_re * _re + _im * _im);
        }

        /// <summary>Returns the square of the modulus of the <c>Complex</c></summary>
        /// <returns>a double</returns>
        public double SquareModulus()
        {
            return _re * _re + _im * _im;
        }

        /// <summary>
        /// Returns the argument of the <c>Complex</c> (between -Pi and +Pi)
        /// </summary>
        /// <returns>the argument of the <c>Complex</c></returns>
        public double Argument()
        {
            if (_re == 0)
                return 0.5 * Math.Sign(_im) * Math.PI;
            if (_im == 0)
                return Math.Sign(_re) * Math.PI;

            if (_re > 0)
                return Math.Atan(_im / _re);
            else
                return Math.Atan(_im / _re) + Math.Sign(_im) * Math.PI;
        }

        /// <summary>
        /// Returns a string that represents the <c>Complex</c>
        /// </summary>
        /// <returns>a string that represents the <c>Complex</c></returns>
        public override string ToString()
        {
            if (_im == 0) return _re.ToString();
            return string.Format("{0}{1}i{2}", _re, (_im > 0 ? "+" : "-"), Math.Abs(_im));
        }

        /// <summary>
        /// returns a string that represents the <c>Complex</c> with the specified format to the composants
        /// </summary>
        /// <param name="format">the format string</param>
        /// <returns>a string that represents the <c>Complex</c></returns>
        public string ToString(string format)
        {
            if (_im == 0) return _re.ToString(format);
            return string.Format("{0}{1}i{2}", _re.ToString(format), (_im > 0 ? "+" : "-"), Math.Abs(_im).ToString(format));
        }
        #endregion

        #region Helpers
        /// <summary>
        /// Returns the imaginary number i
        /// </summary>
        public static Complex I
        {
            get { return new Complex(0, 1); }
        }

        /// <summary>
        /// Returns the complex representation of the real number 1
        /// </summary>
        public static Complex One
        {
            get { return new Complex(1, 0); }
        }
        
        /// <summary>
        /// Returns a complex with both imaginary and real parts equal to zero
        /// </summary>
        public static Complex Zero
        {
            get { return new Complex(0, 0); }
        }

        /// <summary>Returns a complex from its exponential form </summary>
        /// <param name="c">the exponential argument</param>
        /// <returns>a <c>Complex</c></returns>
        public static Complex Exp(Complex c)
        {
            double f = Math.Exp(c._re),
                newRe = Math.Cos(c._im),
                newIm = Math.Sin(c._im);
            return new Complex(f * newRe, f * newIm);
        }
        #endregion

        #region Operators

        /// <summary>
        /// Allows to add a complex to another complex
        /// </summary>
        /// <param name="x">the right hand side</param>
        /// <param name="y">the left hand side</param>
        /// <returns>the <c>Complex</c> result of the addition</returns>
        public static Complex operator +(Complex x, Complex y)
        {
            return new Complex(x._re + y._re, x._im + y._im);
        }

        /// <summary>
        /// Allows to add a scalar to a complex
        /// </summary>
        /// <param name="c">the complex left hand side</param>
        /// <param name="d">the scalar right hand side</param>
        /// <returns>the <c>Complex</c> result of the addition</returns>
        public static Complex operator +(Complex c, double d)
        {
            return new Complex(c._re + d, c._im);
        }

        /// <summary>
        /// Allows to add a scalar to a complex
        /// </summary>
        /// <param name="d">the scalar right hand side </param>
        /// <param name="c">the complex left hand side</param>
        /// <returns>the <c>Complex</c> result of the addition</returns>
        public static Complex operator +(double d, Complex c)
        {
            return c + d;
        }

        /// <summary>
        /// Allows to substract a complex to another complex
        /// </summary>
        /// <param name="x">the right hand side</param>
        /// <param name="y">the left hand side</param>
        /// <returns>the <c>Complex</c> result of the substraction</returns>
        public static Complex operator -(Complex x, Complex y)
        {
            return x + (-1) * y;
        }

        /// <summary>
        /// Allows to multiply a complex by another complex
        /// </summary>
        /// <param name="x">the left hand side</param>
        /// <param name="y">the right hand side</param>
        /// <returns>the <c>Complex</c> result of the multiplication</returns>
        public static Complex operator *(Complex x, Complex y)
        {
            return new Complex(x._re * y._re - x._im * y._im, x._im * y._re + y._im * x._re);
        }

        /// <summary>
        /// Allows to multiply a complex by a scalar
        /// </summary>
        /// <param name="c">the complex number</param>
        /// <param name="d">the scalar</param>
        /// <returns>the <c>Complex</c> result of the multiplication</returns>
        public static Complex operator *(Complex c, double d)
        {
            return new Complex(c._re * d, c._im * d);
        }

        /// <summary>
        /// Allows to multiply a complex by a scalar
        /// </summary>
        /// <param name="d">the scalar</param>
        /// <param name="c">the complex number</param>
        /// <returns>the <c>Complex</c> result of the multiplication</returns>
        public static Complex operator *(double d, Complex c)
        {
            return c * d;
        }

        /// <summary>
        /// Allows to divide a complex by another complex
        /// </summary>
        /// <param name="x">the numerator</param>
        /// <param name="y">the denominator</param>
        /// <returns>the <c>Complex</c> result of the division</returns>
        public static Complex operator /(Complex x, Complex y)
        {
            return x * y.Conjugate * (1.0 / y.SquareModulus());
        }
        #endregion
    }
}
