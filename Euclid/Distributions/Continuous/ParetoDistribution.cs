using System;

namespace Euclid.Distributions.Continuous
{
    public class ParetoDistribution : ContinousDistribution
    {
        #region Declarations
        private double _xm, _alpha;
        #endregion

        #region Constructors
        private ParetoDistribution(double xm, double alpha, Random randomSource)
        {
            if (xm <= 0) throw new ArgumentException("xm has to be positive");
            if (alpha <= 0) throw new ArgumentException("alpha has to be positive");
            _alpha = alpha;
            _xm = xm;
            if (randomSource == null) throw new ArgumentException("The random source can not be null");
            _randomSource = randomSource;
        }

        public ParetoDistribution(double xm, double alpha)
            : this(xm, alpha, new Random(DateTime.Now.Millisecond))
        { }
        #endregion

        #region Accessors
        public override double Entropy
        {
            get { return Math.Log((_xm / _alpha) * Math.Exp(1 + 1 / _alpha)); }
        }
        public override double Maximum
        {
            get { return double.MaxValue; }
        }
        public override double Mean
        {
            get
            {
                if (_alpha <= 1) return double.MaxValue;
                else return _alpha * _xm / (_alpha - 1);
            }
        }
        public override double Median
        {
            get { return _xm * Math.Pow(2, 1 / _alpha); }
        }
        public override double Minimum
        {
            get { return _xm; }
        }
        public override double Mode
        {
            get { return _xm; }
        }
        public override double Skewness
        {
            get
            {
                if (_alpha <= 3) return double.MaxValue;
                else return 2 * (1 + _alpha) / (_alpha - 3) * Math.Sqrt((_alpha - 2) / _alpha);
            }
        }
        public override double StandardDeviation
        {
            get
            {
                if (_alpha <= 2) return double.MaxValue;
                else return (_xm / (_alpha - 1)) * Math.Sqrt(_alpha / (_alpha - 2));
            }
        }
        public override double Variance
        {
            get
            {
                if (_alpha <= 2) return double.MaxValue;
                else return Math.Pow(_xm / (_alpha - 1), 2) * _alpha / (_alpha - 2);
            }
        }
        #endregion

        #region Methods
        public override double CumulativeDistribution(double x)
        {
            if (x >= _xm) return Math.Pow(1 - (_xm / x), _alpha);
            else return 0;
        }
        public override double InverseCumulativeDistribution(double p)
        {
            return _xm / Math.Exp(Math.Log(1 - p) / _alpha);
        }
        public override double ProbabilityDensity(double x)
        {
            if (x >= _xm) return _alpha * Math.Pow(_xm / x, _alpha) / x;
            else return 0;
        }
        public override double[] Sample(int size)
        {
            double[] result = new double[size];
            for (int i = 0; i < size; i++)
                result[i] = _xm / Math.Exp(Math.Log(_randomSource.NextDouble()) / _alpha);
            return result;
        }
        #endregion
    }
}
