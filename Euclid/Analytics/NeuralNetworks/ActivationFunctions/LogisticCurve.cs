using System;

namespace Euclid.Analytics.NeuralNetworks.ActivationFunctions
{
    /// <summary>
    /// The logistic activation function
    /// </summary>
    public class LogisticCurve : IActivationFunction
    {
        /// <summary>The logistic curve</summary>
        /// <param name="x">x</param>
        /// <returns>a double</returns>
        public double Function(double x)
        {
            return 1 / (1 + Math.Exp(-x));
        }

        /// <summary>The logistic curve's derivative</summary>
        /// <param name="x">x</param>
        /// <returns>a double</returns>
        public double Derivative(double x)
        {
            double e = Math.Exp(-x);
            return e / Math.Pow(1 + e, 2);
        }

        /// <summary>The maximum value of the activation function</summary>
        public double Max => +1;

        /// <summary>The minimum value of the activation function</summary>
        public double Min => 0;
    }
}
