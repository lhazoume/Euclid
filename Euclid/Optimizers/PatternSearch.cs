using Euclid.Extensions;
using Euclid.Solvers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Euclid.Optimizers
{
    /// <summary>Pattern search class</summary>
    public class PatternSearch
    {
        #region Variables
        private readonly Vector _initialPoint, _initialShocks;
        private readonly int _maxIterations, _maxStaticIterations;
        private SolverStatus _status;
        private readonly OptimizationType _optimizationType;
        private readonly double _shrinkageFactor;
        private double _epsilon;

        private readonly Func<Vector, double> _fitnessFunction;
        private readonly Func<Vector, bool> _isFeasible;

        private Vector _result;
        private readonly List<Tuple<Vector, double>> _convergence;
        #endregion

        /// <summary>Builds a Pattern Search Optimizer</summary>
        /// <param name="fitnessFunction">the function to optimize</param>
        /// <param name="feasabilityFunction">the feasability function</param>
        /// <param name="optimizationType">the optimization type</param>
        /// <param name="initialPoint">the initial point</param>
        /// <param name="shocks">the shocks' values</param>
        /// <param name="maxIterations">the maximum number of iterations</param>
        /// <param name="maxStaticIterations">the maximum number of static iterations</param>
        /// <param name="epsilon">the convergence threshold</param>
        /// <param name="shrinkageFactor">the shrinkage factor</param>
        public PatternSearch(Func<Vector, bool> feasabilityFunction,
            Func<Vector, double> fitnessFunction,
            Vector initialPoint, Vector shocks,
            OptimizationType optimizationType,
            int maxIterations,
            int maxStaticIterations,
            double epsilon = 1e-8,
            double shrinkageFactor = 0.5)
        {
            #region Check the initial point
            if (shocks.Data.Min() <= 0)
                throw new ArgumentOutOfRangeException(nameof(shocks), "The shocks should be positive");

            if (!feasabilityFunction(initialPoint))
                throw new ArgumentOutOfRangeException(nameof(initialPoint), "The initial point is not feasible");

            _initialPoint = initialPoint;
            _initialShocks = shocks;
            #endregion

            #region Feasibility and fitness
            _isFeasible = feasabilityFunction;
            _fitnessFunction = fitnessFunction;
            #endregion

            _shrinkageFactor = shrinkageFactor;
            _optimizationType = optimizationType;

            if (epsilon <= 0)
                throw new ArgumentOutOfRangeException(nameof(epsilon), "The epsilon should all be >0");
            _epsilon = epsilon;

            if (maxIterations <= 0 || maxStaticIterations <= 0)
                throw new ArgumentException("The maximum number of iterations and static iterations should both be >0");
            _maxIterations = maxIterations;
            _maxStaticIterations = maxStaticIterations;

            _fitnessFunction = fitnessFunction;
            _convergence = new List<Tuple<Vector, double>>();
            _status = SolverStatus.NotRan;
        }

        #region Optimization params

        /// <summary>Gets the tolerance used to check if the optimization process is stationary</summary>
        public double Epsilon
        {
            get { return _epsilon; }
            set
            {
                if (value > 0)
                    _epsilon = value;
                else
                    throw new ArgumentException("The epsilon has to be >=0");
            }
        }

        /// <summary>Gets the maximum number of iterations allowed to the optimization process</summary>
        public int MaxIterations => _maxIterations;

        /// <summary>Gets the maximum number of stationary iterations allowed to the optimization process</summary>
        public int MaxStaticIterations => _maxStaticIterations;

        /// <summary>Gets the current status of the optimization process</summary>
        public SolverStatus Status => _status;

        /// <summary>Gets the type of optimization performed (min or max)</summary>
        public OptimizationType OptimizationType => _optimizationType;

        #endregion

        #region PS params
        /// <summary>Gets the shrinkage factor</summary>
        public double ShrinkageFactor => _shrinkageFactor;

        /// <summary>The result of the solver</summary>
        public Vector Result => _result;

        /// <summary>Gets the details of the convergence (Vector, error)</summary>
        public List<Tuple<Vector, double>> Convergence => _convergence.ToList();
        #endregion

        /// <summary>Optimizes the function using Pattern Search</summary>
        public void Optimize(bool parallel)
        {
            #region Define the general vars
            int sign = _optimizationType == OptimizationType.Min ? -1 : 1;
            Vector current = _initialPoint.Clone,
                shock = _initialShocks.Clone;
            #endregion

            double reference = _fitnessFunction(current);

            _convergence.Clear();
            _convergence.Add(new Tuple<Vector, double>(current.Clone, reference));

            EndCriteria endCriteria = new EndCriteria(maxIterations: _maxIterations,
                maxStaticIterations: _maxStaticIterations,
                gradientEpsilon: _epsilon);

            while (!endCriteria.ShouldStop(reference))
            {
                Tuple<Vector, double>[] neighbours = new Tuple<Vector, double>[2 * _initialShocks.Size];
                #region Compute neighbours
                Loops.For(0, _initialShocks.Size, parallel, i =>
                {
                    Vector up = current.Clone,
                        dn = current.Clone;
                    up[i] += shock[i];
                    dn[i] -= shock[i];

                    if (_isFeasible(up))
                        neighbours[i] = new Tuple<Vector, double>(up, _fitnessFunction(up));
                    if (_isFeasible(dn))
                        neighbours[i + _initialShocks.Size] = new Tuple<Vector, double>(dn, _fitnessFunction(dn));
                });
                #endregion

                List<Tuple<Vector, double>> relevantNeighbours = neighbours.Where(t => t != null && Math.Sign(t.Item2 - reference) == sign).ToList();
                if (relevantNeighbours.Count == 0)
                    shock *= _shrinkageFactor;
                else
                {
                    double target = _optimizationType == OptimizationType.Min ? relevantNeighbours.Min(t => t.Item2) : relevantNeighbours.Max(t => t.Item2);
                    current = relevantNeighbours.Find(t => t.Item2 == target).Item1.Clone;
                    reference = _fitnessFunction(current);
                }

                _convergence.Add(new Tuple<Vector, double>(current, reference));
            }

            _result = current;
            _status = endCriteria.Status;
        }
    }
}
