using System;

namespace Euclid
{
    public sealed class Complex
    {
        #region Declarations
        private double _re, _im;
        #endregion

        #region Constructors
        public Complex(double real, double imaginary)
        {
            _re = real;
            _im = imaginary;
        }
        public Complex()
            : this(0, 0)
        { }
        #endregion

        #region Accessors
        public double Re
        {
            get { return _re; }
            set { _re = value; }
        }
        public double Im
        {
            get { return _im; }
            set { _im = value; }
        }
        public Complex Conjugate
        {
            get { return new Complex(_re, -_im); }
        }
        #endregion

        #region Methods
        public double Module()
        {
            return Math.Sqrt(_re * _re + _im * _im);
        }
        public double SquareModule()
        {
            return _re * _re + _im * _im;
        }
        public double Argument()
        {
            if (_re == 0)
                return 0.5 * Math.PI;
            return Math.Atan(_im / _re);
        }
        public override string ToString()
        {
            if (_im == 0) return _re.ToString();
            return string.Format("{0}{1}i{2}", _re, (_im > 0 ? "+" : "-"), Math.Abs(_im));
        }
        public string ToString(string format)
        {
            if (_im == 0) return _re.ToString(format);
            return string.Format("{0}{1}i{2}", _re.ToString(format), (_im > 0 ? "+" : "-"), Math.Abs(_im).ToString(format));
        }
        #endregion

        #region Helpers
        public static Complex I
        {
            get { return new Complex(0, 1); }
        }
        public static Complex One
        {
            get { return new Complex(1, 0); }
        }
        public static Complex Zero
        {
            get { return new Complex(0, 0); }
        }
        public static Complex Exp(Complex c)
        {
            double f = Math.Exp(c._re),
                newRe = Math.Cos(c._im),
                newIm = Math.Sin(c._im);
            return new Complex(f * newRe, f * newIm);
        }
        #endregion

        #region Operators
        public static Complex operator +(Complex x, Complex y)
        {
            return new Complex(x._re + y._re, x._im + y._im);
        }
        public static Complex operator +(Complex c, double d)
        {
            return new Complex(c._re + d, c._im);
        }
        public static Complex operator +(double d, Complex c)
        {
            return c + d;
        }
        public static Complex operator -(Complex x, Complex y)
        {
            return x + (-1) * y;
        }

        public static Complex operator *(Complex x, Complex y)
        {
            return new Complex(x._re * y._re - x._im * y._im, x._im * y._re + y._im * x._re);
        }
        public static Complex operator *(Complex c, double d)
        {
            return new Complex(c._re * d, c._im * d);
        }
        public static Complex operator *(double d, Complex c)
        {
            return c * d;
        }
        public static Complex operator /(Complex x, Complex y)
        {
            return x * y.Conjugate * (1.0 / y.SquareModule());
        }
        #endregion
    }
}
