namespace Euclid.Analytics.NeuralNetworks.ActivationFunctions
{
    /// <summary>
    /// Activation function interface
    /// </summary>
    public interface IActivationFunction
    {
        /// <summary>The activation function</summary>
        /// <param name="x">x</param>
        /// <returns>a double</returns>
        double Function(double x);

        /// <summary>The activation function's derivative</summary>
        /// <param name="x">x</param>
        /// <returns>a double</returns>
        double Derivative(double x);

        /// <summary>The maximum value of the activation function</summary>
        double Max { get; }

        /// <summary>The minimum value of the activation function</summary>
        double Min { get; }
    }
}
