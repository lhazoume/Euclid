namespace Euclid.Distributions.Continuous.Kernels
{
    /// <summary>
    /// Interface for all density kernels
    /// </summary>
    public interface IDensityKernel
    {
        /// <summary>
        /// the kernel function
        /// </summary>
        /// <param name="x"></param>
        /// <returns>a double</returns>
        double K(double x);
        /// <summary>
        /// the left hand side integral of the kernel function
        /// </summary>
        /// <param name="x"></param>
        /// <returns>a double</returns>
        double IntegralK(double x);

        /// <summary>
        /// Returns the integral of t^2*K(t)
        /// </summary>
        double Variance { get; }
    }
}
