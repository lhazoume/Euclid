﻿using Euclid.Extensions;
using Euclid.Solvers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Euclid.Optimizers
{
    /// <summary>Particle Swarm Optimization class</summary>
    public class ParticleSwarmOptimizer
    {
        #region Variables
        private readonly Vector[] _initialPopulation;
        private readonly int _swarmSize, _maxIterations, _maxStaticIterations;
        private SolverStatus _status;
        private readonly OptimizationType _optimizationType;
        private readonly double _attractionToParticleBest, _attractionToOverallBest,
            _velocityInertia, _shrinkageFactor;
        private double _epsilon;
        private readonly Func<Vector, double> _fitnessFunction;
        private readonly Func<Vector, bool> _isFeasible;
        private Vector _result;
        private readonly List<Tuple<Vector, double>> _convergence;
        #endregion

        /// <summary>Builds a Particle Swarm Optimizer</summary>
        /// <param name="function">the function to optimize</param>
        /// <param name="feasability">the feasability function</param>
        /// <param name="optimizationType">the optimization type</param>
        /// <param name="initialPopulation">a generator of seed vectors</param>
        /// <param name="maxIterations">the maximum number of iterations</param>
        /// <param name="maxStaticIterations">the maximum number of static iterations</param>
        /// <param name="epsilon">the convergence threshold</param>
        /// <param name="attractionToParticleBest">the attraction to particle best factor</param>
        /// <param name="attractionToOverallBest">the attraction to overall best factor</param>
        /// <param name="velocityInertia">the velocity inertia</param>
        /// <param name="shrinkageFactor">the shrinkage factor</param>
        public ParticleSwarmOptimizer(Func<Vector, bool> feasability,
            Func<Vector, double> function,
            IEnumerable<Vector> initialPopulation,
            OptimizationType optimizationType,
            int maxIterations,
            int maxStaticIterations,
            double epsilon = 1e-8,
            double attractionToParticleBest = 2, double attractionToOverallBest = 2,
            double velocityInertia = 0.5, double shrinkageFactor = 0.5)
        {
            #region Check and initialize the population of agents
            if (initialPopulation.Count() <= 1)
                throw new ArgumentOutOfRangeException(nameof(initialPopulation), "The swarm size should be at least 2");

            _initialPopulation = initialPopulation.Where(feasability).ToArray();
            _swarmSize = _initialPopulation.Length;
            #endregion

            #region Feasibility and fitness
            _isFeasible = feasability;
            _fitnessFunction = function;
            #endregion

            if (attractionToOverallBest < 0 || attractionToParticleBest < 0 || velocityInertia < 0)
                throw new ArgumentException("The attraction to overall best, attraction to particle best, velocity's inertia should all be >=0");
            _attractionToOverallBest = attractionToOverallBest;
            _attractionToParticleBest = attractionToParticleBest;
            _velocityInertia = velocityInertia;
            _shrinkageFactor = shrinkageFactor;

            _optimizationType = optimizationType;

            if (epsilon <= 0)
                throw new ArgumentOutOfRangeException(nameof(epsilon), "The epsilon should all be >0");
            _epsilon = epsilon;

            if (maxIterations <= 0 || maxStaticIterations <= 0)
                throw new ArgumentException("The maximum number of iterations and static iterations should both be >0");
            _maxIterations = maxIterations;
            _maxStaticIterations = maxStaticIterations;

            _fitnessFunction = function;
            _convergence = new List<Tuple<Vector, double>>();
            _status = SolverStatus.NotRan;
        }

        /// <summary>Builds a Particle Swarm Optimizer</summary>
        /// <param name="function">the function to optimize</param>
        /// <param name="optimizationType">the optimization type</param>
        /// <param name="initialPopulation">a generator of seed vectors</param>
        /// <param name="maxIterations">the maximum number of iterations</param>
        /// <param name="maxStaticIterations">the maximum number of static iterations</param>
        /// <param name="epsilon">the convergence threshold</param>
        /// <param name="attractionToParticleBest">the attraction to particle best factor</param>
        /// <param name="attractionToOverallBest">the attraction to overall best factor</param>
        /// <param name="velocityInertia">the velocity inertia</param>
        public ParticleSwarmOptimizer(Func<Vector, double> function, 
            IEnumerable<Vector> initialPopulation,
            OptimizationType optimizationType,            
            int maxIterations,
            int maxStaticIterations,
            double epsilon = 1e-8, double attractionToParticleBest = 2,
            double attractionToOverallBest = 2, double velocityInertia = 0.5)
        {
            #region Check and initialize the population of agents
            if (initialPopulation.Count() <= 1)
                throw new ArgumentOutOfRangeException(nameof(initialPopulation), "The swarm size should be at least 2");

            _initialPopulation = initialPopulation.ToArray();
            _swarmSize = _initialPopulation.Length;
            #endregion

            #region Feasibility and fitness
            _isFeasible = v => true;
            _fitnessFunction = function;
            #endregion

            if (attractionToOverallBest < 0 || attractionToParticleBest < 0 || velocityInertia < 0)
                throw new ArgumentException("The attraction to overall best, attraction to particle best, velocity's inertia should all be >=0");
            _attractionToOverallBest = attractionToOverallBest;
            _attractionToParticleBest = attractionToParticleBest;
            _velocityInertia = velocityInertia;

            _optimizationType = optimizationType;

            if (epsilon <= 0)
                throw new ArgumentOutOfRangeException(nameof(epsilon), "The epsilon should all be >0");
            _epsilon = epsilon;

            if (maxIterations <= 0 || maxStaticIterations <= 0)
                throw new ArgumentException("The maximum number of iterations and static iterations should both be >0");
            _maxIterations = maxIterations;
            _maxStaticIterations = maxStaticIterations;

            _fitnessFunction = function;
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

        #region PSO params
        /// <summary> Gets the size of the swarm</summary>
        public int SwarmSize => _swarmSize;

        /// <summary>Gets the inertia applied to the velocity</summary>
        public double VelocityInertia => _velocityInertia;

        /// <summary>Gets the attraction to particle best</summary>
        public double AttractionToParticleBest => _attractionToParticleBest;

        /// <summary>Gets the attraction to overall best</summary>
        public double AttractionToOverallBest => _attractionToOverallBest;

        /// <summary>The result of the solver</summary>
        public Vector Result => _result;

        /// <summary>Gets the details of the convergence (Vector, error)</summary>
        public IEnumerable<Tuple<Vector, double>> Convergence => _convergence;
        #endregion

        /// <summary>Optimizes the function using Particle Swarm Optimization</summary>
        public void Optimize(bool parallel)
        {
            #region Define the general vars
            int sign = _optimizationType == OptimizationType.Min ? -1 : 1;
            Vector[] swarm = _initialPopulation.Select(v => v.Clone).ToArray(),
                velocities = _initialPopulation.Select(v => Vector.Create(v.Size)).ToArray(),
                particleBests = _initialPopulation.Select(v => v.Clone).ToArray();
            Vector personalBestsValues = Vector.Create(_swarmSize);
            Vector overallBest;
            double overallBestValue;
            #endregion

            #region Initialize the swarm and bests
            Loops.For(0, _swarmSize, parallel, i => { personalBestsValues[i] = _fitnessFunction(swarm[i]); });

            overallBestValue = _optimizationType == OptimizationType.Min ? personalBestsValues.Data.Min() : personalBestsValues.Data.Max();
            overallBest = swarm[Array.IndexOf(personalBestsValues.Data, overallBestValue)].Clone;
            #endregion

            _convergence.Clear();
            _convergence.Add(new Tuple<Vector, double>(overallBest.Clone, overallBestValue));

            EndCriteria endCriteria = new EndCriteria(maxIterations: _maxIterations,
                maxStaticIterations: _maxStaticIterations,
                gradientEpsilon: _epsilon);

            while (!endCriteria.ShouldStop(overallBestValue))
            {
                //prepare queue of random numbers


                #region Move all the particles in the swarm
                Loops.For(0, _swarmSize, parallel, i =>
                {
                    Vector towardsParticleBest = particleBests[i] - swarm[i],
                        towardsOverallBest = overallBest - swarm[i],
                        inertia = _velocityInertia * velocities[i],
                        newVelocity;
                    Random rnd = new Random(Guid.NewGuid().GetHashCode());

                    double shrinkage = 1;
                    do
                    {
                        newVelocity = shrinkage * Vector.Create(rnd.NextDouble(), inertia,
                            _attractionToParticleBest * rnd.NextDouble(), towardsParticleBest,
                            _attractionToOverallBest * rnd.NextDouble(), towardsOverallBest);
                        shrinkage *= _shrinkageFactor;
                    }
                    while (!_isFeasible(swarm[i] + newVelocity));

                    velocities[i] = newVelocity;
                    swarm[i] += newVelocity;

                    double value = _fitnessFunction(swarm[i]);
                    if (!double.IsInfinity(value) && Math.Sign(value - personalBestsValues[i]) == sign)
                    {
                        particleBests[i] = swarm[i].Clone;
                        personalBestsValues[i] = value;
                    }
                });
                #endregion

                #region Find the overall best
                double currentOverallBest = _optimizationType == OptimizationType.Min ? personalBestsValues.Data.Min() : personalBestsValues.Data.Max();
                if (Math.Sign(currentOverallBest - overallBestValue) == sign)
                {
                    overallBestValue = currentOverallBest;
                    overallBest = swarm[Array.IndexOf(personalBestsValues.Data, currentOverallBest)].Clone;
                }
                #endregion

                _convergence.Add(new Tuple<Vector, double>(overallBest.Clone, overallBestValue));
            }

            _result = overallBest;
            _status = endCriteria.Status;
        }
    }
}
