using Euclid.Distributions.Continuous;
using Euclid.Distributions.Continuous.Kernels;
using Euclid.Histograms;
using Euclid.Solvers;
using System;
using System.Linq;

namespace Euclid.Distributions
{
    public class EmpiricUnivariateDistribution : ContinuousDistribution
    {
        #region Declarations
        private readonly double[] _weights, _values;
        private readonly double[,] _buckets;
        private readonly double _h, _sumWeights, _average;
        private readonly int _n;
        private readonly IDensityKernel _kernel;
        #endregion

        private EmpiricUnivariateDistribution(double[] weights, double[] values, double h, IDensityKernel kernel, Random randomSource)
        {
            if (weights == null || values == null ||
                weights.Length == 0 || values.Length == 0 ||
                weights.Length != values.Length)
                throw new ArgumentException("The weights and values are not right");
            _n = weights.Length;
            _weights = new double[_n];

            _values = new double[_n];
            _h = h;
            _kernel = kernel;

            if (randomSource == null) throw new ArgumentException("The random source can not be null");
            _randomSource = randomSource;

            _sumWeights = 0;
            _average = 0;
            for (int i = 0; i < _n; i++)
            {
                _weights[i] = weights[i];
                _sumWeights += _weights[i];
                _values[i] = values[i];
                _average += _weights[i] * _values[i];
            }
            _average /= _sumWeights;

            #region Prepare for the inverse CDF
            double[] orderedValues = _values.Distinct().OrderBy(d => d).ToArray();
            _buckets = new double[orderedValues.Length + 2, 2];
            _buckets[0, 0] = _values.Min() - _h;
            _buckets[0, 1] = 0;
            for (int i = 0; i < orderedValues.Length; i++)
            {
                _buckets[1 + i, 0] = orderedValues[i];
                _buckets[1 + i, 1] = CumulativeDistribution(orderedValues[i]);
            }
            _buckets[orderedValues.Length + 1, 0] = _values.Max() + _h;
            _buckets[orderedValues.Length + 1, 1] = 1;
            #endregion

            _support = new Interval(double.NegativeInfinity, double.PositiveInfinity, false, false);
        }

        #region Creators
        public static EmpiricUnivariateDistribution Create(double[] weights, double[] values, double h, IDensityKernel kernel)
        {
            return new EmpiricUnivariateDistribution(weights, values, h, kernel, new Random(DateTime.Now.Millisecond));
        }
        #endregion

        #region Accessors

        public override double Median { get { return InverseCumulativeDistribution(0.5); } }
        public override double Mean { get { return _average; } }
        public override double Mode { get { return _values[Array.IndexOf(_weights, _weights.Max())]; } }
        public override double StandardDeviation { get { return Math.Sqrt(Variance); } }
        public override double Variance
        {
            get
            {
                double sum = 0;
                for (int i = 0; i < _weights.Length; i++)
                    sum += _weights[i] * (_values[i] * _values[i] + _h * _h * _kernel.Variance);
                sum = sum / _sumWeights - _average * _average;

                return sum;
            }
        }
        public override Interval Support { get { return _support; } }

        #endregion

        //TODO
        public override double Skewness { get { throw new NotImplementedException(); } }

        //TODO
        public override double Entropy { get { throw new NotImplementedException(); } }


        #region Methods

        public override double CumulativeDistribution(double x)
        {
            double sum = 0;
            for (int i = 0; i < _weights.Length; i++)
                sum += _weights[i] * _kernel.IntegralK((x - _values[i]) / _h);
            return sum / (_sumWeights);
        }
        public override double InverseCumulativeDistribution(double p)
        {
            int i = 0;
            while (_buckets[i, 1] < p)
                i++;

            RootBracketing solver = new RootBracketing(_buckets[i - 1, 0], _buckets[i, 0], CumulativeDistribution, RootBracketingMethod.Dichotomy, 10000);
            solver.Tolerance = 0.0001;
            solver.Solve(p);
            return solver.Result;
        }
        public override double ProbabilityDensity(double x)
        {
            double sum = 0;
            for (int i = 0; i < _weights.Length; i++)
                sum += _weights[i] * _kernel.K((x - _values[i]) / _h);
            return sum / (_h * _sumWeights);
        }

        #endregion


    }
}
