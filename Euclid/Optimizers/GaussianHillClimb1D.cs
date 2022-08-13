using Euclid.DataStructures;
using Euclid.Distributions.Continuous;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Euclid.Optimizers
{
    /// <summary>Gaussian hill-climbing optimization  algorithm for a 1D function</summary>
    public class GaussianHillClimb1D
    {
        #region Declarations
        private bool _trackConvergence;
        private readonly double _sigma0, _gamma;
        private double _initialGuess, _result = 0;
        private readonly List<Tuple<double, double>> _convergence;
        private Func<double, double> _fitness;
        private readonly Func<double, bool> _feasibility;
        private int _iterations;
        private readonly OptimizationType _optimizationType;
        #endregion

        /// <summary>Builds a solver using the simulated annealing method</summary>
        /// <param name="initialGuess">the initial guess</param>
        /// <param name="fitness">the function to solve for</param>
        /// <param name="feasibility">the feasibility function</param>
        /// <param name="sigma0">the initial standard deviation</param>
        /// <param name="gamma">the cooling down speed</param>
        /// <param name="iterations">the maximum number of iterations</param>
        /// <param name="optimizationType">the optimization type</param>
        public GaussianHillClimb1D(double initialGuess, Func<double, double> fitness,
            Func<double, bool> feasibility,
            OptimizationType optimizationType,
            double sigma0, double gamma,
            int iterations)
        {
            _trackConvergence = false;
            _initialGuess = initialGuess;
            _feasibility = feasibility;
            _sigma0 = sigma0;
            _gamma = gamma;
            _fitness = fitness;
            _iterations = iterations;
            _optimizationType = optimizationType;
            _convergence = new List<Tuple<double, double>>();
        }

        /// <summary>Builds a solver using the simulated annealing method</summary>
        /// <param name="initialGuess">the initial guess</param>
        /// <param name="fitness">the function to solve for</param>
        /// <param name="optimizationType">the optimization type</param>
        /// <param name="lowerBound">the lower bound of the searching scope</param>
        /// <param name="upperBound">the upper bound of the searching scope</param>
        /// <param name="tolerance">the final accuracy</param>
        /// <param name="iterations">the maximum number of iterations</param>
        public GaussianHillClimb1D(double initialGuess, Func<double, double> fitness,
            OptimizationType optimizationType,
            double lowerBound, double upperBound,
            double tolerance,
            int iterations)
            : this(initialGuess, fitness, x => x < upperBound && x > lowerBound, optimizationType, (upperBound - lowerBound) / 2,
                  -Math.Log(2 * tolerance / (upperBound - lowerBound)),
                  iterations)
        { }

        #region Accessors

        #region Settables
        /// <summary>Gets and sets the function to solve for</summary>
        public Func<double, double> Function
        {
            get { return _fitness; }
            set { _fitness = value; }
        }

        /// <summary>Gets and sets the initial guess</summary>
        public double InitialGuess
        {
            get { return _initialGuess; }
            set { _initialGuess = value; }
        }

        /// <summary>Gets and sets the number of iterations</summary>
        public int Iterations
        {
            get { return _iterations; }
            set
            {
                if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value), "The maximum number of iterations should be positive");
                _iterations = value;
            }
        }

        /// <summary>Gets and sets whether the details of the convergence are tracked</summary>
        public bool TrackConvergence
        {
            get => _trackConvergence;
            set => _trackConvergence = value;
        }
        #endregion

        #region Get

        /// <summary> Gets the result of the solver</summary>
        public double Result => _result;

        /// <summary>Gets the details of the convergence (value, error)</summary>
        public IEnumerable<Tuple<double, double>> Convergence => _convergence;

        #endregion

        #endregion

        #region Methods
        /// <summary>Optimizes the function</summary>
        public void Optimize()
        {
            int optimizationSign = _optimizationType == OptimizationType.Max ? +1 : -1;
            double currentPoint = _initialGuess,
                value = _fitness(currentPoint);
            _convergence.Clear();

            NormalDistribution normalDistribution = new NormalDistribution();
            EndlessStack<double> sample = new EndlessStack<double>(normalDistribution.Sample, _iterations, Math.Max(50, _iterations / 10));

            for (int i = 0; i < _iterations; i++)
            {
                double stdev = _sigma0 * Math.Exp(-_gamma * i / (_iterations - 1));

                #region Pull a feasible random point
                double randomPoint;
                do { randomPoint = currentPoint + stdev * sample.Pop(); }
                while (!_feasibility(randomPoint));
                #endregion

                double newValue = _fitness(randomPoint);
                if (Math.Sign(newValue - value) == optimizationSign)
                {
                    currentPoint = randomPoint;
                    value = newValue;

                    if (_trackConvergence)
                        _convergence.Add(new Tuple<double, double>(currentPoint, value));
                }
            }

            _result = currentPoint;
        }

        #endregion
    }
}
