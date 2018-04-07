using System;

namespace Euclid.Solvers
{
    public class Function
    {
        #region Variables
        private Func<Vector, double> _value;
        private Func<Vector, Vector> _gradient;
        #endregion

        /// <summary>Builds an analytically differentiable function</summary>
        /// <param name="value">the value function</param>
        /// <param name="gradient">the gradient</param>
        public Function(Func<Vector, double> value, Func<Vector, Vector> gradient)
        {
            _value = value;
            _gradient = gradient;
        }

        public Func<Vector, double> Value
        {
            get { return _value; }
        }
        public Func<Vector, Vector> Gradient
        {
            get { return _gradient; }
        }

        #region Operators
        public static Function operator *(Function func, double factor)
        {
            return new Function(v => func.Value(v) * factor, v => func.Gradient(v) * factor);
        }
        public static Function operator *(double factor, Function func)
        {
            return func * factor;
        }

        public static Function operator +(Function func, double qty)
        {
            return new Function(v => func.Value(v) + qty, v => func.Gradient(v));
        }
        public static Function operator +(double qty, Function func)
        {
            return func + qty;
        }

        public static Function operator -(Function func, double qty)
        {
            return func + (-qty);
        }
        public static Function operator -(double qty, Function func)
        {
            return ((-1) * func) + qty;
        }

        public static Function operator /(Function func, double dividor)
        {
            return func * (1 / dividor);
        }

        public static Function operator +(Function func1, Function func2)
        {
            return new Function(v => func1.Value(v) + func2.Value(v), v => func1.Gradient(v) + func2.Gradient(v));
        }
        public static Function operator -(Function func1, Function func2)
        {
            return new Function(v => func1.Value(v) - func2.Value(v), v => func1.Gradient(v) - func2.Gradient(v));
        }
        public static Function operator /(Function func1, Function func2)
        {
            return new Function(v => func1.Value(v) / func2.Value(v), v => (func1.Gradient(v) / func2.Value(v)) - (func2.Gradient(v) * (func1.Value(v) * Math.Pow(func2.Value(v), -2))));
        }
        #endregion

        #region Composed
        /// <summary>Elevates a function to a power</summary>
        /// <param name="func">the function</param>
        /// <param name="pow">the power</param>
        /// <returns>a Function</returns>
        public static Function Power(Function func, double pow)
        {
            return new Function(v => Math.Pow(func.Value(v), pow), v => pow * Math.Pow(func.Value(v), pow - 1) * func.Gradient(v));
        }

        /// <summary>Applies square root to a Function</summary>
        /// <param name="func">the function</param>
        /// <returns>a Function</returns>
        public static Function Sqrt(Function func)
        {
            return Power(func, 0.5);
        }

        /// <summary>Applies exponential to a Function</summary>
        /// <param name="func">the function</param>
        /// <returns>a Function</returns>
        public static Function Exp(Function func)
        {
            return new Function(v => Math.Exp(func.Value(v)), v => func.Gradient(v) * func.Value(v));
        }

        /// <summary>Applies Log to a Function</summary>
        /// <param name="func">the function</param>
        /// <returns>a Function</returns>
        public static Function Log(Function func)
        {
            return new Function(v => Math.Log(func.Value(v)), v => (1 / func.Value(v)) * func.Gradient(v));
        }
        #endregion

        #region Base
        public static Function Zero
        {
            get { return new Function(v => 0, v => Vector.Create(v.Size)); }
        }

        /// <summary>Builds a function operating a scalar product vs the given vector</summary>
        /// <param name="alt">the vector</param>
        /// <returns>a Function</returns>
        public static Function Scalar(Vector alt)
        {
            return new Function(v => Vector.Scalar(v, alt), v => alt);
        }
        public static Function Quadratic(Matrix a)
        {
            return new Function(v => Vector.Quadratic(v, a, v), v => v * (a + a.Transpose));
        }
        #endregion
    }
}
