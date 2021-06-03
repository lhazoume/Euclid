using Euclid.DataStructures;
using Euclid.Distributions.Continuous;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Euclid.Optimizers
{
    /// <summary>Simulated annealing optimization class </summary>
    public class SimulatedAnnealing
    {
        #region Declarations
        private Func<Vector, double> _f;
        private readonly Func<int, double> _temperatureFunc;
        private readonly Func<Vector, bool> _isFeasible;
        private readonly Func<Vector, Vector> _neighbourGenerator;

        private bool _trackConvergence;
        private readonly List<Tuple<Vector, double>> _convergence;
        private Vector _initialGuess, _result;

        private int _maxIterations;
        private readonly OptimizationType _optimizationType;
        #endregion

        /// <summary>Builds a solver using the simulated annealing method</summary>
        /// <param name="initialGuess">the initial guess</param>
        /// <param name="f">the function to optimize for</param>
        /// <param name="feasibility">defines the feasible territory</param>
        /// <param name="temperatureFunc">provides the cooling trajectory</param>
        /// <param name="neighbourGenerator">generates random neighbours</param>
        /// <param name="maxIterations">the maximum number of iterations</param>
        /// <param name="optimizationType">the optimization type</param>
        public SimulatedAnnealing(Vector initialGuess, Func<Vector, double> f,
            Func<Vector, bool> feasibility,
            Func<int, double> temperatureFunc,
            Func<Vector, Vector> neighbourGenerator,
            OptimizationType optimizationType,
            int maxIterations)
        {
            _trackConvergence = false;
            _convergence = new List<Tuple<Vector, double>>();
            _initialGuess = initialGuess;
            _f = f;
            _isFeasible = feasibility;

            _neighbourGenerator = neighbourGenerator;
            _temperatureFunc = temperatureFunc;

            _maxIterations = maxIterations;
            _optimizationType = optimizationType;
        }

        #region Accessors

        #region Settables
        /// <summary>Gets and sets the function to solve for</summary>
        public Func<Vector, double> Function
        {
            get => _f;
            set => _f = value;
        }

        /// <summary>Gets and sets the initial guess</summary>
        public Vector InitialGuess
        {
            get => _initialGuess;
            set => _initialGuess = value;
        }

        /// <summary>Gets and sets the maximum number of iterations</summary>
        public int MaxIterations
        {
            get { return _maxIterations; }
            set
            {
                if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value), "The maximum number of iterations should be positive");
                _maxIterations = value;
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
        public Vector Result => _result;

        /// <summary>Gets the details of the convergence (value, error)</summary>
        public List<Tuple<Vector, double>> Convergence => _convergence.ToList();

        #endregion

        #endregion

        #region Methods
        /// <summary>Optimizes the function</summary>
        public void Optimize()
        {
            int optimizationSign = _optimizationType == OptimizationType.Max ? +1 : -1;

            Vector currentPoint = _initialGuess;
            double value = _f(currentPoint);
            _convergence.Clear();

            UniformDistribution standardUniform = new UniformDistribution();
            EndlessStack<double> sample = new EndlessStack<double>(standardUniform.Sample, _maxIterations, Math.Max(_maxIterations / 10, 50));

            for (int i = 0; i < _maxIterations; i++)
            {
                #region Pull ramndom but feasible neighbour
                Vector neighbour;
                do { neighbour = _neighbourGenerator(currentPoint); }
                while (!_isFeasible(neighbour));
                #endregion


                double newValue = _f(neighbour);
                if (Math.Sign(newValue - value) == optimizationSign     // goes downhill
                    ||
                    Math.Exp(optimizationSign * (newValue - value) / _temperatureFunc(i)) > sample.Pop()) //allows uphill if the temperature is high enough
                {
                    currentPoint = neighbour;
                    value = newValue;

                    if (_trackConvergence)
                        _convergence.Add(new Tuple<Vector, double>(currentPoint, value));
                }
            }

            _result = currentPoint;
        }

        #endregion
    }
}
