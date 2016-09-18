using System.Collections.Generic;

namespace Euclid.Analytics.ErrorFunctions
{
    /// <summary>
    /// Interface for error function
    /// </summary>
    public interface IErrorFunction
    {
        /// <summary>
        /// Calculates the error
        /// </summary>
        /// <param name="x">the obtained vector</param>
        /// <param name="benchmark">the expected vector</param>
        /// <returns>the error</returns>
        double Function(Vector x, Vector benchmark);

        /// <summary>
        /// Calculates the sum of the errors
        /// </summary>
        /// <param name="x">the list of obtained vectors</param>
        /// <param name="benchmark">the list of expected vectors</param>
        /// <returns>the sum of the errors</returns>
        double Function(List<Vector> x, List<Vector> benchmark);

        /// <summary>
        /// Calculates the gradient of the error function, as observed from the obtained vector's value
        /// </summary>
        /// <param name="x">the obtained vector</param>
        /// <param name="benchmark">the obtained vector</param>
        /// <returns>a vector whose size is the same as the obtained vector</returns>
        Vector Gradient(Vector x, Vector benchmark);
    }
}
