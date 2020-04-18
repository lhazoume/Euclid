using System;

namespace Euclid
{
    /// <summary>Represents complex numbers</summary>
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
        /// <summary>Returns the real part of the <c>Complex</c></summary>
        public double Re
        {
            get { return _re; }
            set { _re = value; }
        }

        /// <summary>Returns the imaginary part of the <c>Complex</c></summary>
        public double Im
        {
            get { return _im; }
            set { _im = value; }
        }

        /// <summary>The conjugate number of the <c>Complex</c></summary>
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
            if (_re == 0) return 0.5 * Math.Sign(_im) * Math.PI;
            if (_im == 0) return _re > 0 ? 0 : Math.PI;

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
            return string.Format("{0}{1}i{2}", _re, (_im > 0 ? "+" : "-"), (Math.Abs(_im) == 1 ? "" : Math.Abs(_im).ToString()));
        }

        /// <summary>
        /// returns a string that represents the <c>Complex</c> with the specified format to the composants
        /// </summary>
        /// <param name="format">the format string</param>
        /// <returns>a string that represents the <c>Complex</c></returns>
        public string ToString(string format)
        {
            if (_im == 0) return _re.ToString(format);
            return string.Format("{0}{1}i{2}", _re.ToString(format), (_im > 0 ? "+" : "-"), (Math.Abs(_im) == 1 ? "" : Math.Abs(_im).ToString(format)));
        }
        #endregion

        #region Helpers
        /// <summary>Returns the imaginary number <c>i</c></summary>
        public static Complex I
        {
            get { return new Complex(0, 1); }
        }

        /// <summary>Returns the complex representation of the real number 1</summary>
        public static Complex One
        {
            get { return new Complex(1, 0); }
        }

        /// <summary>Returns a complex with both imaginary and real parts equal to zero</summary>
        public static Complex Zero
        {
            get { return new Complex(0, 0); }
        }

        /// <summary>Returns a complex from its exponential form </summary>
        /// <param name="complex">the exponential argument</param>
        /// <returns>a <c>Complex</c></returns>
        public static Complex Exp(Complex complex)
        {
            if (complex == null) throw new ArgumentNullException(nameof(complex));
            double f = Math.Exp(complex._re),
                newRe = Math.Cos(complex._im),
                newIm = Math.Sin(complex._im);
            return new Complex(f * newRe, f * newIm);
        }
        #endregion

        #region Operators

        /// <summary>Allows to add a complex to another complex</summary>
        /// <param name="lhs">the right hand side</param>
        /// <param name="rhs">the left hand side</param>
        /// <returns>the <c>Complex</c> result of the addition</returns>
        public static Complex operator +(Complex lhs, Complex rhs)
        {
            if (lhs == null) throw new ArgumentNullException(nameof(lhs), "the left hand side is null");
            if (rhs == null) throw new ArgumentNullException(nameof(rhs), "the right hand side is null");

            return new Complex(lhs._re + rhs._re, lhs._im + rhs._im);
        }


        /// <summary>Allows to add a scalar to a complex</summary>
        /// <param name="complex">the complex left hand side</param>
        /// <param name="addon">the scalar right hand side</param>
        /// <returns>the <c>Complex</c> result of the addition</returns>
        private static Complex Add(Complex complex, double addon)
        {
            if (complex == null) throw new ArgumentNullException(nameof(complex));
            return new Complex(complex._re + addon, complex._im);
        }

        /// <summary>Allows to add a scalar to a complex</summary>
        /// <param name="complex">the complex left hand side</param>
        /// <param name="addon">the scalar right hand side</param>
        /// <returns>the <c>Complex</c> result of the addition</returns>
        public static Complex operator +(Complex complex, double addon)
        {
            return Add(complex, addon);
        }

        /// <summary>Allows to add a scalar to a complex</summary>
        /// <param name="addon">the scalar right hand side </param>
        /// <param name="complex">the complex left hand side</param>
        /// <returns>the <c>Complex</c> result of the addition</returns>
        public static Complex operator +(double addon, Complex complex)
        {
            return Add(complex, addon);
        }

        /// <summary>Allows to substract a complex to another complex</summary>
        /// <param name="rhs">the right hand side</param>
        /// <param name="lhs">the left hand side</param>
        /// <returns>the <c>Complex</c> result of the substraction</returns>
        public static Complex operator -(Complex lhs, Complex rhs)
        {
            return lhs + (-1) * rhs;
        }

        /// <summary>Allows to multiply a complex by another complex</summary>
        /// <param name="lhs">the left hand side</param>
        /// <param name="rhs">the right hand side</param>
        /// <returns>the <c>Complex</c> result of the multiplication</returns>
        public static Complex operator *(Complex lhs, Complex rhs)
        {
            if (lhs == null) throw new ArgumentNullException(nameof(lhs));
            if (rhs == null) throw new ArgumentNullException(nameof(rhs));
            return new Complex(lhs._re * rhs._re - lhs._im * rhs._im, lhs._im * rhs._re + rhs._im * lhs._re);
        }

        /// <summary>Allows to multiply a complex by a scalar</summary>
        /// <param name="complex">the complex number</param>
        /// <param name="factor">the scalar</param>
        /// <returns>the <c>Complex</c> result of the multiplication</returns>
        public static Complex operator *(Complex complex, double factor)
        {
            if (complex == null) throw new ArgumentNullException(nameof(complex));
            return new Complex(complex._re * factor, complex._im * factor);
        }

        /// <summary>Allows to multiply a complex by a scalar</summary>
        /// <param name="factor">the scalar</param>
        /// <param name="complex">the complex number</param>
        /// <returns>the <c>Complex</c> result of the multiplication</returns>
        public static Complex operator *(double factor, Complex complex)
        {
            return complex * factor;
        }

        /// <summary>Allows to divide a complex by another complex</summary>
        /// <param name="numerator">the numerator</param>
        /// <param name="denominator">the denominator</param>
        /// <returns>the <c>Complex</c> result of the division</returns>
        public static Complex operator /(Complex numerator, Complex denominator)
        {
            if (denominator == null) throw new ArgumentNullException(nameof(denominator));
            return numerator * denominator.Conjugate * (1.0 / denominator.SquareModulus());
        }

        /// <summary>Allows to divide a complex by a double</summary>
        /// <param name="complex">the numerator</param>
        /// <param name="factor">the denominator</param>
        /// <returns>the <c>Complex</c> result of the division</returns>
        public static Complex operator /(Complex complex, double factor)
        {
            return complex * (1.0 / factor);
        }

        /// <summary>Returns the opposite of the complex</summary>
        /// <param name="complex">the <c>Complex</c></param>
        /// <returns>the opposite complex</returns>
        public static Complex operator -(Complex complex)
        {
            if (complex == null) throw new ArgumentNullException(nameof(complex));
            return new Complex(-complex._re, -complex._im);
        }

        #endregion
    }
}
