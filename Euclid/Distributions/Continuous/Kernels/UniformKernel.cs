using System;

namespace Euclid.Distributions.Continuous.Kernels
{
    /// <summary>
    /// Uniform kernel
    /// </summary>
    public class UniformKernel : IDensityKernel
    {
        /// <summary>
        /// the kernel function
        /// </summary>
        /// <param name="x"></param>
        /// <returns>a double</returns>
        public double K(double x)
        {
            if (Math.Abs(x) >= 1) return 0;
            return 0.5;
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
            return 0.5 * (1 + x);
        }

        /// <summary>
        /// Returns the integral of t^2*K(t)
        /// </summary>
        public double Variance
        {
            get { return 1.0 / 3.0; }
        }
    }
}
