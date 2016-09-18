using System;
using System.Collections.Generic;

namespace Euclid.Analytics.ErrorFunctions
{
    /// <summary>
    /// Sum of squares error function
    /// </summary>
    public class ManhattanError : IErrorFunction
    {
        /// <summary>
        /// Calculates the Manhattan norm of the difference between the obtained vector and the expected vector
        /// </summary>
        /// <param name="x">the obtained vector</param>
        /// <param name="benchmark">the expected vector</param>
        /// <returns>the error</returns>
        public double Function(Vector x, Vector benchmark)
        {
            return (x - benchmark).Norm1;
        }

        /// <summary>
        /// Calculates the sum of the Manhattan norms of the differences between the obtained and the expected vectors
        /// </summary>
        /// <param name="x">the list of obtained vectors</param>
        /// <param name="benchmark"></param>
        /// <returns>the sum of the errors</returns>
        public double Function(List<Vector> x, List<Vector> benchmark)
        {
            double sum = 0;
            for (int i = 0; i < x.Count; i++)
                sum += (x[i] - benchmark[i]).Norm1;
            return sum;
        }

        /// <summary>
        /// Calculates the gradient of the Manhattan norm, as observed from the obtained vector's value
        /// </summary>
        /// <param name="x">the obtained vector</param>
        /// <param name="benchmark">the obtained vector</param>
        /// <returns>a vector whose size is the same as the obtained vector</returns>
        public Vector Gradient(Vector x, Vector benchmark)
        {
            return Vector.Apply(x - benchmark, d => Math.Sign(d));
        }
    }
}
