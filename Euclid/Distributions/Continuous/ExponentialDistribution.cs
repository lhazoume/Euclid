using System;

namespace Euclid.Distributions.Continuous
{
    public class ExponentialDistribution : ContinousDistribution
    {
        #region Declarations
        private double _lambda, _beta;
        private Random _randomSource;
        #endregion

        #region Constructors
        private ExponentialDistribution(double lambda, Random randomSource)
        {
            if (lambda <= 0) throw new ArgumentException("λ has to be positive");
            _lambda = lambda;
            _beta = 1 / _lambda;
            if (randomSource == null) throw new ArgumentException("The random source can not be null");
            _randomSource = randomSource;
        }
        public ExponentialDistribution(double lambda)
            : this(lambda, new Random(DateTime.Now.Millisecond))
        { }
        #endregion

        #region Methods
        public override double CumulativeDistribution(double x)
        {
            if (x < 0) return 0;
            else return 1 - Math.Exp(-_lambda * x);
        }
        public override double InverseCumulativeDistribution(double p)
        {
            return -Math.Log(1 - p) * _beta;
        }
        public override double ProbabilityDensity(double x)
        {
            if (x < 0) return 0;
            else return _lambda * Math.Exp(-_lambda * x);
        }
        public override double[] Sample(int size)
        {
            double[] result = new double[size];
            for (int i = 0; i < size; i++)
                result[i] = -Math.Log(_randomSource.NextDouble()) * _beta;
            return result;
        }
        #endregion

        #region Accessors
        public override double Minimum
        {
            get { return 0; }
        }
        public override double Entropy
        {
            get { return Math.Log(Math.E * _beta); }
        }
        public override double Maximum
        {
            get { return double.MaxValue; }
        }
        public override double Mean
        {
            get { return _beta; }
        }
        public override double Median
        {
            get { return _beta * Math.Log(2); }
        }
        public override double Mode
        {
            get { return 0; }
        }
        public override double Skewness
        {
            get { return 2; }
        }
        public override double Variance
        {
            get { return _beta * _beta; }
        }
        public override double StandardDeviation
        {
            get { return _beta; }
        }
        #endregion
    }
}
