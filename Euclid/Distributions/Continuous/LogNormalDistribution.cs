using System;

namespace Euclid.Distributions.Continuous
{
    public class LogNormalDistribution : ContinousDistribution
    {
        #region Declarations
        private double _mu, _sigma, _sigma2;
        #endregion

        #region Constructors
        private LogNormalDistribution(double mu, double sigma, Random randomSource)
        {
            if (sigma <= 0) throw new ArgumentException("sigma has to be positive");
            _sigma = sigma;
            _sigma2 = _sigma * _sigma;
            _mu = mu;
            if (randomSource == null) throw new ArgumentException("The random source can not be null");
            _randomSource = randomSource;
        }
        public LogNormalDistribution(double mu, double sigma)
            : this(mu, sigma, new Random(DateTime.Now.Millisecond))
        { }
        #endregion

        #region Accessors
        public override double Entropy
        {
            get { return Math.Log(_sigma * Math.Exp(_mu + 0.5) * Math.Sqrt(2 * Math.PI)); }
        }
        public override double Maximum
        {
            get { return double.MaxValue; }
        }
        public override double Mean
        {
            get { return Math.Exp(_mu + 0.5 * _sigma2); }
        }
        public override double Median
        {
            get { return Math.Exp(_mu); }
        }
        public override double Minimum
        {
            get { return 0; }
        }
        public override double Mode
        {
            get { return Math.Exp(_mu - _sigma2); }
        }
        public override double Skewness
        {
            get { return (Math.Exp(_sigma2) + 2) * Math.Sqrt(Math.Exp(_sigma2) - 1); }
        }
        public override double StandardDeviation
        {
            get { return Math.Sqrt(Variance); }
        }
        public override double Variance
        {
            get { return (Math.Exp(_sigma2) - 1) * Math.Exp(2 * _mu + _sigma2); }
        }
        #endregion

        #region Methods
        public override double CumulativeDistribution(double x)
        {
            if (x <= 0) return 0;
            else return NormalDistribution.CumulativeDistributionFunction(0, 1, (Math.Log(x) - _mu) / _sigma);
        }
        public override double InverseCumulativeDistribution(double p)
        {
            return Math.Exp(_mu + _sigma * NormalDistribution.InverseCumulativeDistributionFunction(0, 1, p));
        }
        public override double ProbabilityDensity(double x)
        {
            if (x <= 0) return 0;
            else return Math.Exp(-0.5 * Math.Pow(Math.Log(x) - _mu, 2) / _sigma2) / (x * _sigma * Math.Sqrt(2 * Math.PI));
        }
        public override double[] Sample(int size)
        {
            double[] result = new double[size];
            for (int i = 0; i < size; i++)
                result[i] = Math.Exp(_mu + _sigma * NormalDistribution.InverseCumulativeDistributionFunction(0, 1, Math.Log(_randomSource.NextDouble())));
            return result;
        }
        #endregion
    }
}
