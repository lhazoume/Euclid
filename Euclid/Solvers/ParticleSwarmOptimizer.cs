using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Euclid.Solvers
{
    /// <summary>Particle Swarm Optimization class</summary>
    public class ParticleSwarmOptimizer
    {
        private readonly Vector _lowerBound, _upperBound;
        private int _swarmSize, _maxIterations, _maxStaticIterations, _dimension;
        private SolverStatus _status;
        private OptimizationType _optimizationType;

        private double _attractionToParticleBest, _attractionToOverallBest, _velocityInertia, _epsilon;
        private Func<Vector, double> _function;
        private Func<int, Vector[]> _generator;
        private Vector _result;
        private List<Tuple<Vector, double>> _convergence = new List<Tuple<Vector, double>>();

        /// <summary>Builds a Particle Swarm Optimizer</summary>
        /// <param name="swarmSize">the swarm size</param>
        /// <param name="lowerBounds">the lower bounds of the space</param>
        /// <param name="upperBounds">the upper bounds of the space</param>
        /// <param name="function">the function to optimize</param>
        /// <param name="optimizationType">the optimization type</param>
        /// <param name="generator">a generator of seed vectors</param>
        /// <param name="maxIterations">the maximum number of iterations</param>
        /// <param name="maxStaticIterations">the maximum number of static iterations</param>
        /// <param name="epsilon">the convergence threshold</param>
        /// <param name="attractionToParticleBest">the attraction to particle best factor</param>
        /// <param name="attractionToOverallBest">the attraction to overall best factor</param>
        /// <param name="velocityInertia">the velocity inertia</param>
        public ParticleSwarmOptimizer(int swarmSize, Vector lowerBounds, Vector upperBounds,
            Func<Vector, double> function,
            OptimizationType optimizationType,
            Func<int, Vector[]> generator,
            int maxIterations,
            int maxStaticIterations,
            double epsilon = 1e-8, double attractionToParticleBest = 2, double attractionToOverallBest = 2, double velocityInertia = 0.5)
        {
            if (swarmSize <= 1)
                throw new ArgumentOutOfRangeException("The swarm size should be at least 2");
            _swarmSize = swarmSize;

            if (lowerBounds.Size != upperBounds.Size) throw new RankException("The lower and upper boundsshould be the same size");
            _lowerBound = lowerBounds.Clone;
            _upperBound = upperBounds.Clone;
            _dimension = _lowerBound.Size;

            if (attractionToOverallBest < 0 || attractionToParticleBest < 0 || velocityInertia < 0)
                throw new ArgumentOutOfRangeException("The attraction to overall best, attraction to particle best, velocity's inertia should all be >=0");
            _attractionToOverallBest = attractionToOverallBest;
            _attractionToParticleBest = attractionToParticleBest;
            _velocityInertia = velocityInertia;


            _optimizationType = optimizationType;

            if (epsilon <= 0)
                throw new ArgumentOutOfRangeException("The epsilon should all be >0");
            _epsilon = epsilon;

            if (maxIterations <= 0 || maxStaticIterations <= 0)
                throw new ArgumentOutOfRangeException("The maximum number of iterations and static iterations should both be >0");
            _maxIterations = maxIterations;
            _maxStaticIterations = maxStaticIterations;

            _function = function;
            _generator = generator;
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
        public int MaxIterations
        {
            get { return _maxIterations; }
        }

        /// <summary>Gets the maximum number of stationary iterations allowed to the optimization process</summary>
        public int MaxStaticIterations
        {
            get { return _maxStaticIterations; }
        }

        /// <summary>Gets the current status of the optimization process</summary>
        public SolverStatus Status
        {
            get { return _status; }
        }

        /// <summary>Gets the type of optimization performed (min or max)</summary>
        public OptimizationType OptimizationType { get { return _optimizationType; } }

        #endregion

        #region PSO params
        /// <summary> Gets the size of the swarm</summary>
        public int SwarmSize
        {
            get { return _swarmSize; }
        }

        /// <summary>Gets the dimension of each particle of the swarm</summary>
        public int Dimension
        {
            get { return _dimension; }
        }

        /// <summary>Gets the lower bounds of the space in which the particles evolve</summary>
        public Vector LowerBound
        {
            get { return _lowerBound; }
        }

        /// <summary>Gets the upper bounds of the space in which the particles evolve</summary>
        public Vector UpperBound
        {
            get { return _upperBound; }
        }

        /// <summary>Gets the inertia applied to the velocity</summary>
        public double VelocityInertia
        {
            get { return _velocityInertia; }
        }

        /// <summary>Gets the attraction to particle best</summary>
        public double AttractionToParticleBest
        {
            get { return _attractionToParticleBest; }
        }

        /// <summary>Gets the attraction to overall best</summary>
        public double AttractionToOverallBest
        {
            get { return _attractionToOverallBest; }
        }

        /// <summary>The result of the solver</summary>
        public Vector Result
        {
            get { return _result; }
        }

        /// <summary>Gets the details of the convergence (Vector, error)</summary>
        public List<Tuple<Vector, double>> Convergence
        {
            get { return new List<Tuple<Vector, double>>(_convergence); }
        }
        #endregion

        /// <summary>Minimizes the function using Particle Swarm Optimization</summary>
        public void Solve()
        {
            int sign = _optimizationType == OptimizationType.Min ? -1 : 1;
            Vector[] swarm = new Vector[_swarmSize],
                velocities = new Vector[_swarmSize],
                particleBests = new Vector[_swarmSize];
            Vector personalBestsValues = Vector.Create(_swarmSize);

            Vector overallBest;
            double overallBestValue;

            #region Initialize the swarm
            Random rnd = new Random(Guid.NewGuid().GetHashCode());

            Vector[] randomVectors = _generator(_swarmSize);

            Parallel.For(0, _swarmSize, i =>
            {
                swarm[i] = randomVectors[i];
                velocities[i] = Vector.Create(_dimension, 0);

                particleBests[i] = randomVectors[i].Clone;
                personalBestsValues[i] = _function(randomVectors[i]);
            });

            overallBestValue = _optimizationType == OptimizationType.Min ? personalBestsValues.Data.Min() : personalBestsValues.Data.Max();
            overallBest = swarm[Array.IndexOf(personalBestsValues.Data, overallBestValue)].Clone;
            #endregion

            _convergence.Clear();
            _convergence.Add(new Tuple<Vector, double>(overallBest.Clone, overallBestValue));

            EndCriteria endCriteria = new EndCriteria(maxIterations: _maxIterations, maxStaticIterations: _maxStaticIterations, gradientEpsilon: _epsilon);
            while (!endCriteria.ShouldStop(overallBestValue))
            {
                #region Go through the swarm
                for (int i = 0; i < _swarmSize; i++)
                {
                    velocities[i] = Vector.Create(_velocityInertia, velocities[i],
                        _attractionToParticleBest * rnd.NextDouble(), particleBests[i] - swarm[i],
                        _attractionToOverallBest * rnd.NextDouble(), overallBest - swarm[i]);
                    swarm[i] = Vector.Max(_lowerBound, Vector.Min(swarm[i] + velocities[i], _upperBound));

                    double value = _function(swarm[i]);
                    if (Math.Sign(value - personalBestsValues[i]) == sign)
                    {
                        particleBests[i] = swarm[i].Clone;
                        personalBestsValues[i] = value;

                        if (Math.Sign(value - overallBestValue) == sign)
                        {
                            overallBestValue = value;
                            overallBest = swarm[i].Clone;
                        }
                    }
                }
                #endregion
                _convergence.Add(new Tuple<Vector, double>(overallBest.Clone, overallBestValue));
            }

            _result = overallBest;
            _status = endCriteria.Status;
        }

        public void SolveFaster()
        {
            #region Define the general vars
            int sign = _optimizationType == OptimizationType.Min ? -1 : 1;
            Vector[] swarm = new Vector[_swarmSize],
                velocities = new Vector[_swarmSize],
                particleBests = new Vector[_swarmSize];
            Vector personalBestsValues = Vector.Create(_swarmSize);
            Vector overallBest;
            double overallBestValue;
            #endregion

            #region Initialize the swarm and bests (parallel)
            Vector[] randomVectors = _generator(_swarmSize);

            Parallel.For(0, _swarmSize, i =>
            {
                swarm[i] = randomVectors[i];
                velocities[i] = Vector.Create(_dimension, 0);

                particleBests[i] = randomVectors[i].Clone;
                personalBestsValues[i] = _function(randomVectors[i]);
            });

            overallBestValue = _optimizationType == OptimizationType.Min ? personalBestsValues.Data.Min() : personalBestsValues.Data.Max();
            overallBest = swarm[Array.IndexOf(personalBestsValues.Data, overallBestValue)].Clone;
            #endregion

            _convergence.Clear();
            _convergence.Add(new Tuple<Vector, double>(overallBest.Clone, overallBestValue));

            Random rnd = new Random(Guid.NewGuid().GetHashCode());

            EndCriteria endCriteria = new EndCriteria(maxIterations: _maxIterations, maxStaticIterations: _maxStaticIterations, gradientEpsilon: _epsilon);
            while (!endCriteria.ShouldStop(overallBestValue))
            {
                #region Prepare Queue
                double[] randomNumbers = Enumerable.Range(0, 2 * _swarmSize).Select(i => rnd.NextDouble()).ToArray();
                #endregion

                #region Move all the particles in the swarm
                Parallel.For(0, _swarmSize, i =>
                {
                    velocities[i] = Vector.Create(_velocityInertia, velocities[i],
                        _attractionToParticleBest * randomNumbers[2 * i], particleBests[i] - swarm[i],
                        _attractionToOverallBest * randomNumbers[2 * i + 1], overallBest - swarm[i]);
                    swarm[i] = Vector.Max(_lowerBound, Vector.Min(swarm[i] + velocities[i], _upperBound));

                    double value = _function(swarm[i]);
                    if (Math.Sign(value - personalBestsValues[i]) == sign)
                    {
                        particleBests[i] = swarm[i].Clone;
                        personalBestsValues[i] = value;
                    }
                });
                #endregion

                #region
                for (int i = 0; i < _swarmSize; i++)
                    if (Math.Sign(personalBestsValues[i] - overallBestValue) == sign)
                    {
                        overallBest = swarm[i].Clone;
                        overallBestValue = personalBestsValues[i];
                    }

                #endregion
                _convergence.Add(new Tuple<Vector, double>(overallBest.Clone, overallBestValue));
            }

            _result = overallBest;
            _status = endCriteria.Status;
        }
    }
}
