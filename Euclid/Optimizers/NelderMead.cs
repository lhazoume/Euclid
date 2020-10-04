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
        private readonly int _dimension, _maxIterations;

        private readonly Vector _lowerBound, _upperBound;
        private Vector _result;
        private readonly Func<Vector, double> _function;
        private readonly OptimizationType _optimizationType;

        private readonly Vector[] _initialPopulation;

        private readonly List<Tuple<Vector, double>> _convergence = new List<Tuple<Vector, double>>();
        private SolverStatus _status;

        private readonly double _alpha, _gamma, _rho, _sigma;
        private double _epsilon;

        /// <summary>Builds a Nelder Mead optimizer</summary>
        /// <param name="lowerBounds">the lower bounds of the space</param>
        /// <param name="upperBounds">the upper bounds of the space</param>
        /// <param name="function">the function to optimize</param>
        /// <param name="optimizationType">the optimization type</param>
        /// <param name="initialSimplex">the initial population</param>
        /// <param name="maxIterations">the maximum number of iterations</param>
        /// <param name="epsilon">the convergence threshold</param>
        /// <param name="alpha">the reflection coefficient</param>
        /// <param name="gamma">the expansion coefficient</param>
        /// <param name="rho">the contaction coefficient</param>
        /// <param name="sigma">the shrink coefficient</param>
        public NelderMead(Vector lowerBounds, Vector upperBounds,
            Func<Vector, double> function,
            Vector[] initialSimplex,
            OptimizationType optimizationType,
            int maxIterations,
            double epsilon = 1e-8,
            double alpha = 1, double gamma = 2, double rho = 0.5, double sigma = 0.5)
        {
            if (lowerBounds == null) throw new ArgumentNullException(nameof(lowerBounds));
            if (upperBounds == null) throw new ArgumentNullException(nameof(upperBounds));

            #region Bounds and dimension
            if (lowerBounds.Size != upperBounds.Size)
                throw new RankException("The lower and upper bounds should be the same size");
            _lowerBound = lowerBounds.Clone;
            _upperBound = upperBounds.Clone;
            _dimension = _lowerBound.Size;
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

        #region Nelder Mead params

        /// <summary> Returns the problem's dimension</summary>
        public int Dimension => _dimension;

        /// <summary>Gets the lower bounds of the space</summary>
        public Vector LowerBound => _lowerBound;

        /// <summary>Gets the upper bounds of the space</summary>
        public Vector UpperBound => _upperBound;

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
                {
                    break;
                }

                #region Reflection
                Vector reflectionPoint = Vector.Bound(_lowerBound, _upperBound, Vector.Create(1 + _alpha, centroid, -_alpha, simplex[_dimension].Vector));
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
                    Vector expansionPoint = Vector.Bound(_lowerBound, _upperBound, Vector.Create(1 - _gamma, centroid, _gamma, reflectionPoint));
                    double expansionValue = _function(expansionPoint);
                    simplex[_dimension].Vector = expansionValue < reflectionValue ? expansionPoint : reflectionPoint;
                    simplex[_dimension].Value = expansionValue < reflectionValue ? expansionValue : reflectionValue;

                    iterations++;
                    continue;
                }
                #endregion

                #region Contraction
                Vector contractionPoint = Vector.Bound(_lowerBound, _upperBound, Vector.Create(1 - _rho, centroid, _rho, simplex[_dimension].Vector));
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
                      simplex[s].Vector = Vector.Bound(_lowerBound, _upperBound, Vector.Create(1 - _sigma, bestPoint, _sigma, simplex[s].Vector));
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
