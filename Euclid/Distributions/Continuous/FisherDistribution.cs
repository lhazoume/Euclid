using Euclid.Histograms;
using Euclid.Solvers;
using System;

namespace Euclid.Distributions.Continuous
{
    public class FisherDistribution : ContinuousDistribution
    {
        #region Declarations
        private double _d1, _d2;
        #endregion

        private FisherDistribution(double d1, double d2, Random randomSource)
        {
            if (d1 <= 0) throw new ArgumentException("The d1 can not be negative");
            _d1 = d1;

            if (d2 <= 0) throw new ArgumentException("The d2 can not be negative");
            _d2 = d2;

            if (randomSource == null) throw new ArgumentException("The random source can not be null");
            _randomSource = randomSource;

            _support = new Interval(0, double.PositiveInfinity, true, false);
        }

        public FisherDistribution(double d1, double d2)
            : this(d1, d2, new Random(Guid.NewGuid().GetHashCode()))
        { }

        #region Accessors

        public override double Entropy
        {
            get { throw new NotImplementedException(); }
        }

        public override double Mean
        {
            get { return _d2 > 2 ? _d2 / (_d2 - 2) : double.NaN; }
        }

        public override double Median
        {
            get { return InverseCumulativeDistribution(0.5); }
        }

        public override double Mode
        {
            get { return _d1 > 2 ? (_d1 - 2) * _d2 / (_d1 * (_d2 + 2)) : double.NaN; }
        }

        public override double Skewness
        {
            get { return _d2 > 6 ? (2 * _d1 + _d2 - 2) / (_d2 - 6) * Math.Sqrt(8 * (_d2 - 4) / (_d1 * (_d1 + _d2 - 2))) : double.NaN; }
        }

        public override double StandardDeviation
        {
            get { return _d2 > 4 ? _d2 / (_d2 - 2) * Math.Sqrt(2 * (_d1 + _d2 - 2) / (_d1 * (_d2 - 4))) : double.NaN; }
        }

        public override Interval Support
        {
            get { return _support; }
        }

        public override double Variance
        {
            get { return _d2 > 4 ? 2 * Math.Pow(_d2 / (_d2 - 2), 2) * (_d1 + _d2 - 2) / (_d1 * (_d2 - 4)) : double.NaN; }
        }

        #endregion

        #region Methods

        public override double CumulativeDistribution(double x)
        {
            return Fn.IncompleteRegularizedBeta(_d1 * x / (_d1 * x + _d2), 0.5 * _d1, 0.5 * _d2);
        }

        public override double InverseCumulativeDistribution(double p)
        {
            NewtonRaphson solver = new NewtonRaphson(1, CumulativeDistribution, 100);
            solver.SlopeTolerance = 1e-10;
            solver.Solve(p);
            return solver.Result;
        }

        public override double ProbabilityDensity(double x)
        {
            return Math.Sqrt(Math.Pow(_d1 * x, _d1) * Math.Pow(_d2, _d2) / Math.Pow(_d1 * x + _d2, _d1 + _d2)) / (x * Fn.Beta(0.5 * _d1, 0.5 * _d2));
        }

        #endregion
    }
}
