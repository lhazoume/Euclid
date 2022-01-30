using System;

namespace Euclid
{
    /// <summary>Analytically differentiable function</summary>
    public class Function
    {
        #region Variables
        private readonly Func<Vector, double> _value;
        private readonly Func<Vector, Vector> _gradient;
        #endregion

        #region Constructor

        /// <summary>Builds an analytically differentiable function</summary>
        /// <param name="value">the value function</param>
        /// <param name="gradient">the gradient</param>
        public Function(Func<Vector, double> value, Func<Vector, Vector> gradient)
        {
            _value = value;
            _gradient = gradient;
        }

        #endregion

        #region Accessors

        /// <summary>Returns the value function</summary>
        public Func<Vector, double> Value => _value;

        /// <summary>Returns the gradient function</summary>
        public Func<Vector, Vector> Gradient => _gradient;
        
        #endregion

        #region Operators
        /// <summary>Builds a function made of the product of a function and a scalar</summary>
        /// <param name="func">the function</param>
        /// <param name="factor">the scalar</param>
        /// <returns>a <c>Function</c> </returns>
        public static Function operator *(Function func, double factor)
        {
            return new Function(v => func.Value(v) * factor, v => func.Gradient(v) * factor);
        }

        /// <summary>Builds a function made of the product of a function and a scalar</summary>
        /// <param name="func">the function</param>
        /// <param name="factor">the scalar</param>
        /// <returns>a <c>Function</c> </returns>
        public static Function operator *(double factor, Function func)
        {
            return func * factor;
        }

        /// <summary>Builds a function made of the addition of a function and a scalar</summary>
        /// <param name="func">the function</param>
        /// <param name="qty">the scalar</param>
        /// <returns>a <c>Function</c> </returns>
        public static Function operator +(Function func, double qty)
        {
            return new Function(v => func.Value(v) + qty, v => func.Gradient(v));
        }

        /// <summary>Builds a function made of the addition of a function and a scalar</summary>
        /// <param name="func">the function</param>
        /// <param name="qty">the scalar</param>
        /// <returns>a <c>Function</c> </returns>
        public static Function operator +(double qty, Function func)
        {
            return func + qty;
        }

        /// <summary>Builds a function made of the substraction of a scalar to a function</summary>
        /// <param name="func">the function</param>
        /// <param name="qty">the scalar</param>
        /// <returns>a <c>Function</c> </returns>
        public static Function operator -(Function func, double qty)
        {
            return func + (-qty);
        }

        /// <summary>Builds a function made of the substraction of a function to a scalar</summary>
        /// <param name="func">the function</param>
        /// <param name="qty">the scalar</param>
        /// <returns>a <c>Function</c> </returns>
        public static Function operator -(double qty, Function func)
        {
            return ((-1) * func) + qty;
        }

        /// <summary>Builds a function made of the division of a function by a scalar</summary>
        /// <param name="func">the function</param>
        /// <param name="dividor">the dividor</param>
        /// <returns>a <c>Function</c> </returns>
        public static Function operator /(Function func, double dividor)
        {
            return func * (1 / dividor);
        }

        /// <summary>Builds a function made of the addition of two functions</summary>
        /// <param name="lhs">the left-hand side function</param>
        /// <param name="rhs">the right-hand side function</param>
        /// <returns>a <c>Function</c> </returns>
        public static Function operator +(Function lhs, Function rhs)
        {
            return new Function(v => lhs.Value(v) + rhs.Value(v), v => lhs.Gradient(v) + rhs.Gradient(v));
        }

        /// <summary>Builds a function made of the substraction of two functions</summary>
        /// <param name="lhs">the left-hand side function</param>
        /// <param name="rhs">the right-hand side function</param>
        /// <returns>a <c>Function</c> </returns>
        public static Function operator -(Function lhs, Function rhs)
        {
            return new Function(v => lhs.Value(v) - rhs.Value(v), v => lhs.Gradient(v) - rhs.Gradient(v));
        }

        /// <summary>Builds a function made of the division of two functions</summary>
        /// <param name="lhs">the left-hand side function</param>
        /// <param name="rhs">the right-hand side function</param>
        /// <returns>a <c>Function</c> </returns>
        public static Function operator /(Function lhs, Function rhs)
        {
            return new Function(v => lhs.Value(v) / rhs.Value(v), v => (lhs.Gradient(v) / rhs.Value(v)) - (rhs.Gradient(v) * (lhs.Value(v) * Math.Pow(rhs.Value(v), -2))));
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

        /// <summary>Builds a function operating a scalar product vs the given vector</summary>
        /// <param name="alt">the vector</param>
        /// <returns>a Function</returns>
        public static Function Scalar(Vector alt)
        {
            return new Function(v => Vector.Scalar(v, alt), v => alt);
        }

        /// <summary>Builds a function operating a quadratic product over a given vector</summary>
        /// <param name="a">the matrix</param>
        /// <returns>a <c>Function</c></returns>
        public static Function Quadratic(Matrix a)
        {
            return new Function(v => Vector.Quadratic(v, a, v), v => v * (a + a.Transpose));
        }

        #endregion
    }
}
