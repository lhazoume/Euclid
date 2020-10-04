using System;

namespace Euclid.Analytics.NeuralNetworks.ActivationFunctions
{
    /// <summary>
    /// The algebraic activation function
    /// </summary>
    public class Algebraic : IActivationFunction
    {
        /// <summary>The algebraic function</summary>
        /// <param name="x">x</param>
        /// <returns>a double</returns>
        public double Function(double x)
        {
            return x / Math.Sqrt(1 + x * x);
        }

        /// <summary>The algebraic function's derivative</summary>
        /// <param name="x">x</param>
        /// <returns>a double</returns>
        public double Derivative(double x)
        {
            return Math.Pow(1 + x * x, -1.5);
        }

        /// <summary>The maximum value of the activation function</summary>
        public double Max => +1;

        /// <summary>The minimum value of the activation function</summary>
        public double Min => -1;
    }
}
