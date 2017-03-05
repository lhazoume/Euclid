using Euclid.Histograms;

namespace Euclid.Distributions
{
    /// <summary>Interface for all continuous distributions</summary>
    public interface IDistribution
    {
        /// <summary>Evaluates the cumulative distribution function (left hand side)</summary>
        /// <param name="x">the argument</param>
        /// <returns>a double</returns>
        double CumulativeDistribution(double x);

        /// <summary>
        /// Evaluates the cumulative distribution's antecedant for a given probability
        /// </summary>
        /// <param name="p">the probability</param>
        /// <returns>a double</returns>
        double InverseCumulativeDistribution(double p);

        /// <summary>Evaluates the probability density function</summary>
        /// <param name="x">the argument</param>
        /// <returns>a double</returns>
        double ProbabilityDensity(double x);

        /// <summary>
        /// Evaluates the log of the probability density function
        /// </summary>
        /// <param name="x">the argument</param>
        /// <returns>a double</returns>
        double ProbabilityLnDensity(double x);

        /// <summary>Returns the distribution's entropy</summary>
        double Entropy { get; }

        /// <summary>Returns the distribution's mean</summary>
        double Mean { get; }
        
        /// <summary>Returns the distribution's median </summary>
        double Median { get; }

        /// <summary>Returns the distribution's mode</summary>
        double Mode { get; }

        /// <summary>Returns the distribution's skewness</summary>
        double Skewness { get; }

        /// <summary>Returns the distribution's standard deviation</summary>
        double StandardDeviation { get; }

        /// <summary>Returns the distribution's variance</summary>
        double Variance { get; }
    }
}
