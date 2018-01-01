using System;

namespace Euclid.Distributions.Discrete
{
    public class SkellamDistribution : DiscreteDistribution, IParametricDistribution
    {
        #region Declarations
        private double _mu1, _mu2;
        #endregion

        private SkellamDistribution(double mu1, double mu2, Random randomSource)
        {
            if (randomSource == null) throw new ArgumentException("The random source can not be null");
            _randomSource = randomSource;

            if (mu1 <= 0) throw new ArgumentOutOfRangeException("mu1", "The mu1 should be >0");
            _mu1 = mu1;

            if (mu2 <= 0) throw new ArgumentOutOfRangeException("mu2", "The lambda should be >0");
            _mu2 = mu2;
        }

        public SkellamDistribution(double mu1, double mu2)
            : this(mu1, mu2, new Random(Guid.NewGuid().GetHashCode()))
        { }

        #region Accessors

        /// <summary>Gets the distribution's mean</summary>
        public override double Mean { get { return _mu1 - _mu2; } }

        /// <summary>Gets the distribution's skewness</summary>
        public override double Skewness { get { return (_mu1 - _mu2) * Math.Pow(_mu1 + _mu2, 1.5); } }

        /// <summary>Gets the distribution's standard deviation</summary>
        public override double StandardDeviation { get { return Math.Sqrt(_mu1 + _mu2); } }

        /// <summary>Gets the distribution's variance</summary>
        public override double Variance { get { return _mu1 + _mu2; } }

        #endregion

        public override double ProbabilityDensity(double x)
        {
            int k = Convert.ToInt32(Math.Round(x));
            return Math.Exp(-_mu1 - _mu2) * Math.Pow(_mu1 / _mu2, 0.5 * x) * Fn.ik(Math.Abs(k), 2 * Math.Sqrt(_mu1 * _mu2));
        }

        public override double Entropy
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override double Median
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override double Mode
        {
            get
            {
                throw new NotImplementedException();
            }
        }



        public override double[] Support
        {
            get
            {
                throw new NotImplementedException();
            }
        }


        public override double CumulativeDistribution(double x)
        {
            throw new NotImplementedException();
        }

        public void Fit(FittingMethod fitting, double[] sample)
        {
            throw new NotImplementedException();
        }

        public override double InverseCumulativeDistribution(double p)
        {
            throw new NotImplementedException();
        }


    }
}
