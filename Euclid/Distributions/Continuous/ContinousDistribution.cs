using System;

namespace Euclid.Distributions.Continuous
{
    public abstract class ContinousDistribution : IDistribution
    {
        protected Random _randomSource;

        #region Accessors
        public abstract double Entropy { get; }
        public abstract double Maximum { get; }
        public abstract double Mean { get; }
        public abstract double Median { get; }
        public abstract double Minimum { get; }
        public abstract double Mode { get; }
        public abstract double Skewness { get; }
        public abstract double StandardDeviation { get; }
        public abstract double Variance { get; }
        #endregion

        #region Methods
        public abstract double CumulativeDistribution(double x);
        public abstract double InverseCumulativeDistribution(double p);
        public abstract double ProbabilityDensity(double x);
        public double ProbabilityLnDensity(double x)
        {
            return Math.Log(ProbabilityDensity(x));
        }
        #endregion

        public abstract double[] Sample(int size);
    }
}
