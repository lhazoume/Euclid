using Euclid.Solvers;
using System;
using System.Collections.Generic;

namespace Euclid.Numerics
{
    /// <summary>Calculates the sum of the numerical series</summary>
    public class NumericalSeriesCumulator
    {
        #region Declarations
        private long _initialIndex;
        private readonly Func<long, double> _series;
        private double _sum, _tolerance;
        private int _maxIterations, _iterations;
        private NumericalSeriesStatus _status;
        private readonly List<double> _convergence = new List<double>();
        #endregion

        /// <summary>Builds a cumulative calculator of numerical series</summary>
        /// <param name="initialIndex">the initial index of the sum</param>
        /// <param name="series">the series function</param>
        /// <param name="maxIterations">the maximum number of iterations</param>
        public NumericalSeriesCumulator(long initialIndex, Func<long, double> series, int maxIterations)
        {
            _initialIndex = initialIndex;
            _series = series;
            _sum = 0;

            _iterations = 0;
            _maxIterations = maxIterations;
            _tolerance = Descents.ERR_EPSILON;
            _status = NumericalSeriesStatus.NotRan;
        }

        #region Accessors

        #region Settables

        /// <summary>Gets and sets the initial index of the sum</summary>
        public long InitialIndex
        {
            get { return _initialIndex; }
            set { _initialIndex = value; }
        }

        /// <summary>Gets and sets the maximum number of iterations</summary>
        public int MaxIterations
        {
            get { return _maxIterations; }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(value), "The maximum number of iterations should be positive");
                _maxIterations = value;
            }
        }

        /// <summary>
        /// Gets and sets the tolerance for the target (threshold for target reached)
        /// </summary>
        public double Tolerance
        {
            get { return _tolerance; }
            set
            {
                if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value), "The tolerance should be positive");
                _tolerance = value;
            }
        }

        #endregion

        #region Get

        /// <summary>Gets the number of interations of the cumulator</summary>
        public int Iterations
        {
            get { return _iterations; }
        }

        /// <summary>Gets the cumulated sum of the series</summary>
        public double Sum
        {
            get { return _sum; }
        }

        /// <summary>Gets the final status of the cumulator</summary>
        public NumericalSeriesStatus Status
        {
            get { return _status; }
        }

        /// <summary>Gets the details of the convergence (value)</summary>
        public List<double> Convergence
        {
            get { return new List<double>(_convergence); }
        }

        #endregion

        #endregion

        #region Methods

        /// <summary>Sums the series until convergence or exhaustion of the iterations</summary>
        public void Calculate()
        {
            _sum = 0;
            _status = NumericalSeriesStatus.Diverged;
            _convergence.Clear();
            _iterations = 0;
            double term = 1.0;

            while (_iterations < _maxIterations && Math.Abs(term) > _tolerance)
            {
                term = _series(_initialIndex + _iterations);
                _sum += term;
                _convergence.Add(_sum);
                _iterations++;
            }

            if (Math.Abs(term) <= _tolerance)
                _status = NumericalSeriesStatus.Normal;
            else if (_iterations > _maxIterations)
                _status = NumericalSeriesStatus.IterationExceeded;
        }

        /// <summary>Sums the series until convergence or exhaustion of the iterations (using the Aitken Delta Squared method)</summary>
        public void CalculateAitken()
        {
            _sum = 0;
            _status = NumericalSeriesStatus.Diverged;
            _convergence.Clear();
            _iterations = 0;
            double term = 1.0;

            while (_iterations < _maxIterations && Math.Abs(term) > _tolerance)
            {
                long n = _initialIndex + _iterations;
                double[] t = new double[] { _series(n), _series(n + 1), _series(n + 2) };

                term = (t[2] * t[0] - t[1] * t[1]) / (t[2] - 2 * t[1] + t[0]);
                _sum += term;
                _convergence.Add(_sum);
                _iterations++;
            }

            if (Math.Abs(term) <= _tolerance)
                _status = NumericalSeriesStatus.Normal;
            else if (_iterations > _maxIterations)
                _status = NumericalSeriesStatus.IterationExceeded;
        }

        #endregion
    }
}
