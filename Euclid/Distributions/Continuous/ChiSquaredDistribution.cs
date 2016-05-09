using Euclid.Solvers;
using System;

namespace Euclid.Distributions.Continuous
{
    public class ChiSquaredDistribution : ContinousDistribution
    {
        #region Declarations
        private int _k;
        #endregion

        #region Constructors
        private ChiSquaredDistribution(int k, Random randomSource)
        {
            if (k <= 0) throw new ArgumentException("degrees of freedom has to be positive");
            _k = k;
            if (randomSource == null) throw new ArgumentException("The random source can not be null");
            _randomSource = randomSource;
        }
        public ChiSquaredDistribution(int k)
            : this(k, new Random(DateTime.Now.Millisecond))
        { }
        #endregion

        #region Accessors
        [Obsolete]
        public override double Entropy
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override double Maximum
        {
            get { return double.PositiveInfinity; }
        }

        public override double Mean
        {
            get { return _k; }
        }

        public override double Median
        {
            get { return _k * Math.Pow(1 - 2 / (9 * _k), 3); }
        }

        public override double Minimum
        {
            get { return 0; }
        }

        public override double Mode
        {
            get { return Math.Max(_k - 2, 0); }
        }

        public override double Skewness
        {
            get { return Math.Sqrt(8 / _k); }
        }

        public override double StandardDeviation
        {
            get { return Math.Sqrt(2 * _k); }
        }

        public override double Variance
        {
            get { return 2 * _k; }
        }

        #endregion

        #region Methods

        public override double CumulativeDistribution(double x)
        {
            if (x <= 0) return 0;
            return Fn.igam(0.5 * _k, 0.5 * x) / Fn.gamma(0.5 * _k);
        }

        public override double InverseCumulativeDistribution(double p)
        {
            NewtonRaphson solver = new NewtonRaphson(_k, CumulativeDistribution, 10);
            solver.Solve(p);
            return solver.Result;
        }

        public override double ProbabilityDensity(double x)
        {
            return Math.Pow(0.5 * x, 0.5 * _k - 1) * Math.Exp(-0.5 * x) / (x * Fn.gamma(0.5 * _k));
        }

        public override double[] Sample(int size)
        {
            double[] result = new double[size];
            for (int i = 0; i < size; i++)
                result[i] = InverseCumulativeDistribution(_randomSource.NextDouble());
            return result;
        }

        #endregion
    }
}
