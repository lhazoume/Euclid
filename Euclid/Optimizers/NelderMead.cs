using Euclid.Solvers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Euclid.Optimizers
{
    /// <summary>Nelder-Mead optimization class</summary>
    public class NelderMead
    {
        #region Variables
        private readonly int _dimension, _maxIterations;

        private readonly Func<Vector, bool> _feasibility;
        private Vector _result;
        private readonly Func<Vector, double> _function;
        private readonly OptimizationType _optimizationType;

        private readonly Vector[] _initialPopulation;

        private readonly List<Tuple<Vector, double>> _convergence = new List<Tuple<Vector, double>>();
        private SolverStatus _status;

        private readonly double _alpha, _gamma, _rho, _sigma;
        private double _epsilon;
        #endregion

        /// <summary>Builds a Nelder Mead optimizer</summary>
        /// <param name="feasibility">the feasibility function</param>
        /// <param name="function">the function to optimize</param>
        /// <param name="optimizationType">the optimization type</param>
        /// <param name="initialSimplex">the initial population</param>
        /// <param name="maxIterations">the maximum number of iterations</param>
        /// <param name="epsilon">the convergence threshold</param>
        /// <param name="alpha">the reflection coefficient</param>
        /// <param name="gamma">the expansion coefficient</param>
        /// <param name="rho">the contaction coefficient</param>
        /// <param name="sigma">the shrink coefficient</param>
        public NelderMead(Func<Vector, bool> feasibility,
            Func<Vector, double> function,
            IEnumerable<Vector> initialSimplex,
            OptimizationType optimizationType,
            int maxIterations,
            double epsilon = 1e-8, double alpha = 1, double gamma = 2,
            double rho = 0.5, double sigma = 0.5)
        {
            #region Bounds and dimension
            _dimension = initialSimplex.ElementAt(0).Size;
            #endregion

            #region Algorithm parameters
            if (alpha < 0 || gamma < 0 || rho < 0 || sigma < 0)
                throw new ArgumentException("The alpha, gamma, rho, sigma should all be >=0");
            _alpha = alpha;
            _gamma = gamma;
            _rho = rho;
            _sigma = sigma;
            #endregion

            #region Epsilon
            if (epsilon <= 0)
                throw new ArgumentOutOfRangeException(nameof(epsilon), "The epsilon should all be >0");
            _epsilon = epsilon;
            #endregion

            _function = function;
            _feasibility = feasibility;
            _initialPopulation = initialSimplex.ToArray();
            _status = SolverStatus.NotRan;
            _optimizationType = optimizationType;

            if (maxIterations <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxIterations), "The maximum number of iterations should both be >0");
            _maxIterations = maxIterations;
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

        /// <summary>Gets the current status of the optimization process</summary>
        public SolverStatus Status => _status;

        /// <summary>Gets the type of optimization performed (min or max)</summary>
        public OptimizationType OptimizationType => _optimizationType;

        #endregion

        #region Accessors

        /// <summary> Returns the problem's dimension</summary>
        public int Dimension => _dimension;

        /// <summary>Returns the reflexion coefficient</summary>
        public double Alpha => _alpha;

        /// <summary>Returns the expansion coefficient</summary>
        public double Gamma => _gamma;

        /// <summary>Returns the contraction coefficient</summary>
        public double Rho => _rho;

        /// <summary>Returns the shrink coefficient</summary>
        public double Sigma => _sigma;

        /// <summary>The result of the solver</summary>
        public Vector Result => _result;

        /// <summary>Gets the details of the convergence (Vector, error)</summary>
        public List<Tuple<Vector, double>> Convergence => _convergence.ToList();
        #endregion

        /// <summary>Minimizes the function using the Nelder Mead algorithm</summary>
        private class VectorValuePair
        {
            public Vector Vector;
            public double Value;

            public VectorValuePair(Vector vector, double value)
            {
                Vector = vector;
                Value = value;
            }
        }

        /// <summary>Minimizes the function</summary>
        public void Optimize()
        {
            #region Parameters
            List<VectorValuePair> simplex = _initialPopulation.Select(v => new VectorValuePair(v.Clone, _function(v))).ToList();
            Vector centroid = Vector.Create(_dimension);
            #endregion

            int iterations = 0;
            Parallel.For(0, _dimension + 1, s => { simplex[s].Value = _function(simplex[s].Vector); });

            while (iterations < _maxIterations)
            {
                // Evaluate and order the values and indices
                simplex.Sort((x, y) => x.Value.CompareTo(y.Value));

                //Find centroid of the simplex excluding the vertex with highest functionvalue
                centroid = Vector.AggregateSum(simplex.GetRange(0, _dimension).Select(p => p.Vector).ToList()) / _dimension;

                _convergence.Add(new Tuple<Vector, double>(simplex[0].Vector.Clone, simplex[0].Value));
                if (Math.Abs(_function(centroid) - simplex[0].Value) < _epsilon)
                    break;

                #region Reflection
                Vector reflectionPoint;
                double currentAlpha = _alpha;
                do
                {
                    reflectionPoint = Vector.Create(1 + currentAlpha, centroid, -currentAlpha, simplex[_dimension].Vector);
                    currentAlpha /= 2;
                }
                while (!_feasibility(reflectionPoint));

                double reflectionValue = _function(reflectionPoint);
                if (simplex[0].Value <= reflectionValue & reflectionValue < simplex[_dimension - 1].Value)
                {
                    simplex[_dimension].Vector = reflectionPoint;
                    simplex[_dimension].Value = reflectionValue;

                    iterations++;
                    continue;
                }
                #endregion

                #region Expansion
                if (reflectionValue < simplex[0].Value)
                {
                    Vector expansionPoint;
                    double currentGamma = _gamma;
                    do
                    {
                        expansionPoint = Vector.Create(1 - currentGamma, centroid, currentGamma, reflectionPoint);
                        currentGamma /= 2;
                    }
                    while (!_feasibility(expansionPoint));

                    double expansionValue = _function(expansionPoint);
                    simplex[_dimension].Vector = expansionValue < reflectionValue ? expansionPoint : reflectionPoint;
                    simplex[_dimension].Value = expansionValue < reflectionValue ? expansionValue : reflectionValue;

                    iterations++;
                    continue;
                }
                #endregion

                #region Contraction
                Vector contractionPoint;
                double currentRho = _rho;
                do
                {
                    contractionPoint = Vector.Create(1 - currentRho, centroid, currentRho, simplex[_dimension].Vector);
                    currentRho /= 2;
                }
                while (!_feasibility(contractionPoint));

                double contractionValue = _function(contractionPoint);
                if (contractionValue < simplex[_dimension].Value)
                {
                    simplex[_dimension].Vector = contractionPoint;
                    simplex[_dimension].Value = contractionValue;
                    Console.WriteLine(string.Format("Contraction value = {0}", contractionValue));
                    iterations++;
                    continue;
                }
                #endregion

                #region Shrink
                Vector bestPoint = simplex[0].Vector;
                Parallel.For(1, _dimension + 1, s =>
                {
                    Vector finalPoint;
                    double currentSigma = _sigma;
                    do
                    {
                        finalPoint = Vector.Create(1 - currentSigma, bestPoint, currentSigma, simplex[s].Vector);
                        currentSigma /= 2;
                    }
                    while (!_feasibility(finalPoint));

                    simplex[s].Vector = finalPoint;
                    simplex[s].Value = _function(simplex[s].Vector);
                });

                #endregion

                iterations++;
            }

            _result = simplex[0].Vector;
            _status = iterations >= _maxIterations ? SolverStatus.IterationExceeded : SolverStatus.FunctionConvergence;
        }
    }
}
