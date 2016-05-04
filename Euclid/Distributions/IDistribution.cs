namespace Euclid.Distributions
{
    public interface IDistribution
    {
        double CumulativeDistribution(double x);
        double InverseCumulativeDistribution(double p);
        double ProbabilityDensity(double x);
        double ProbabilityLnDensity(double x);

        double Entropy { get; }
        double Maximum { get; }
        double Mean { get; }
        double Median { get; }
        double Minimum { get; }
        double Mode { get; }
        double Skewness { get; }
        double StandardDeviation { get; }
        double Variance { get; }
    }
}
