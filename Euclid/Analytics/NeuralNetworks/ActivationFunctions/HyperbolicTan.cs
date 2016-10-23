using System;

namespace Euclid.Analytics.NeuralNetworks.ActivationFunctions
{
    /// <summary>
    /// The hyperbolic tangent sigmoïd
    /// </summary>
    public class HyperbolicTan : IActivationFunction
    {
        /// <summary>The hyperbolic tangent's function</summary>
        /// <param name="x">x</param>
        /// <returns>a double</returns>
        public double Function(double x)
        {
            double e = Math.Exp(-2 * x);
            return (1 - e) / (1 + e);
        }

        /// <summary>The hyperbolic tangent's derivative</summary>
        /// <param name="x">x</param>
        /// <returns>a double</returns>
        public double Derivative(double x)
        {
            double e = Math.Exp(-2 * x),
                r = 4 * e / Math.Pow(1 + e, 2);
            return r;
        }

        /// <summary>The maximum value of the activation function</summary>
        public double Max
        {
            get { return +1; }
        }

        /// <summary>The minimum value of the activation function</summary>
        public double Min
        {
            get { return -1; }
        }
    }
}
