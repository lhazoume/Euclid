using System;

namespace Euclid.Distributions.Continuous.Kernels
{
    /// <summary>
    /// Epanechnikov kernel
    /// </summary>
    public class EpanechnikovKernel : IDensityKernel
    {
        /// <summary>
        /// the kernel function
        /// </summary>
        /// <param name="x"></param>
        /// <returns>a double</returns>
        public double K(double x)
        {
            if (Math.Abs(x) >= 1) return 0;
            return 0.75 * (1 - x * x);
        }

        /// <summary>
        /// the left hand side integral of the kernel function
        /// </summary>
        /// <param name="x"></param>
        /// <returns>a double</returns>
        public double IntegralK(double x)
        {
            if (x <= -1) return 0;
            if (x >= 1) return 1;
            return 0.75 * x * (1 - x * x / 3) + 0.5;
        }

        /// <summary>Returns the integral of t^2*K(t)</summary>
        public double Variance => 1.0 / 5.0;
    }
}
