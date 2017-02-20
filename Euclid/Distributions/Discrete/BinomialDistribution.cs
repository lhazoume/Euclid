using Euclid.Arithmetics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Euclid.Distributions.Discrete
{
    public class BinomialDistribution : DiscreteDistribution
    {
        #region Declarations
        private double _p, _q;
        private int _n;
        private BinomialCoefficients _bc;
        #endregion

        private BinomialDistribution(int n, double p)
        {
            _p = p;
            _q = 1 - p;
            _n = n;
            _bc = new BinomialCoefficients(_n);
        }

        #region Accessors
        public override double Entropy
        {
            get { return 0.5 * Math.Log(2 * Math.PI * Math.E * _n * _p * _q); }
        }

        public override double Mean
        {
            get { return _n * _p; }
        }

        public override double Median
        {
            get
            {
                double floor = Math.Floor(_n * _p),
                    ceiling = Math.Ceiling(_n * _p);
                return 0.5 * (floor + ceiling);

            }
        }

        public override double Mode
        {
            get { return 0.5 * (Math.Floor((_n + 1) * _p) + Math.Ceiling((_n + 1) * _p) - 1); }
        }

        public override double Skewness
        {
            get { return (1 - 2 * _p) / Math.Sqrt(_n * _p * _q); }
        }

        public override double StandardDeviation
        {
            get { return Math.Sqrt(_n * _p * _q); }
        }

        public override double Variance
        {
            get { return _n * _p * _q; }
        }

        public override double[] Support
        {
            get { return Enumerable.Range(0, _n + 1).Select(d => Convert.ToDouble(d)).ToArray(); }
        }
        #endregion

        public override double CumulativeDistribution(double x)
        {
            if (x < 0) return 0;
            if (x > _n + 1) return 1;
            int k = Convert.ToInt32(Math.Floor(x));
            return Fn.IncompleteRegularizedBeta(_q, _n - k, k + 1);
        }

        public override double InverseCumulativeDistribution(double p)
        {
            if (_p <= 0) return 0;
            if (_p >= 1) return _n;
            int k = 0;

            while (CumulativeDistribution(k) < p)
                k++;
            return k - 1;
        }

        public override double ProbabilityDensity(double x)
        {
            int k = Convert.ToInt32(Math.Round(x));
            if (k == x) return 0;
            if (k < 0 || k > _n) return 0;

            return _bc[k] * Math.Pow(_p / _q, k) * Math.Pow(_q, _n);
        }

    }
}
