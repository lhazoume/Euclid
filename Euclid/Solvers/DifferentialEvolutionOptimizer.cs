﻿using Euclid.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Euclid.Solvers
{
    /// <summary>Optimizes a function on a space using Differential Evolution</summary>
    public class DifferentialEvolutionOptimizer
    {
        #region Variables
        private readonly Vector[] _initialPopulation;
        private readonly int _populationSize, _maxIterations;
        private readonly OptimizationType _optimizationType;
        private readonly Func<Vector, double> _fitnessFunction;
        private readonly Func<Vector, bool> _isFeasable;
        private readonly List<Tuple<Vector, double>> _convergence;
        private readonly double _crossoverProbability, _differentialWeight;
        private Vector _result;
        private SolverStatus _status;
        #endregion

        /// <summary>Builds a differential evolution optimizer for a constrained space</summary>
        /// <param name="initialPopulation">the initial population of agents</param>
        /// <param name="optimizationType">the optimization type (maximize or minimize)</param>
        /// <param name="fitnessFunction">the fitness function</param>
        /// <param name="feasabilityFunction">the feasability function</param>
        /// <param name="maxIterations">the maximum iterations</param>
        /// <param name="crossoverProbability">the crossover probability (handles the frequency of crossover)</param>
        /// <param name="differentialWeight">the differential weight (handles the intensity of mutation)</param>
        public DifferentialEvolutionOptimizer(IEnumerable<Vector> initialPopulation,
            OptimizationType optimizationType,
            Func<Vector, double> fitnessFunction,
            Func<Vector, bool> feasabilityFunction,
            int maxIterations,
            double crossoverProbability, double differentialWeight)
        {
            if (initialPopulation.Count() <= 4)
                throw new ArgumentException("The initial population is too small to perform a differential evolution", "initialPopulation");

            if (initialPopulation.Any(v => !feasabilityFunction(v)))
                throw new ArgumentOutOfRangeException("initialPopulation", "Some agents of the initial population are not feasible");
            _initialPopulation = initialPopulation.ToArray();
            _populationSize = _initialPopulation.Length;

            _isFeasable = feasabilityFunction;
            _fitnessFunction = fitnessFunction;

            _optimizationType = optimizationType;

            if (maxIterations <= 0)
                throw new ArgumentOutOfRangeException("maxIterations", "The maximum number of iterations and static iterations should both be >0");
            _maxIterations = maxIterations;


            if (crossoverProbability >= 1 || crossoverProbability <= 0)
                throw new ArgumentOutOfRangeException("crossoverProbability", "The crossover probability should be positive and below 1");
            _crossoverProbability = crossoverProbability;

            if (differentialWeight == 0)
                throw new ArgumentOutOfRangeException("differentialWeight", "The differential weight should not be equal to zero");

            _differentialWeight = differentialWeight;
            _status = SolverStatus.NotRan;
            _convergence = new List<Tuple<Vector, double>>();
        }

        /// <summary>Builds a differential evolution optimizer for a constrained space</summary>
        /// <param name="initialPopulation">the initial population of agents</param>
        /// <param name="optimizationType">the optimization type (maximize or minimize)</param>
        /// <param name="fitnessFunction">the fitness function</param>
        /// <param name="maxIterations">the maximum iterations</param>
        /// <param name="crossoverProbability">the crossover probability (handles the frequency of crossover)</param>
        /// <param name="differentialWeight">the differential weight (handles the intensity of mutation)</param>
        public DifferentialEvolutionOptimizer(IEnumerable<Vector> initialPopulation,
            OptimizationType optimizationType,
            Func<Vector, double> fitnessFunction,
            int maxIterations,
            double crossoverProbability, double differentialWeight)
            : this(initialPopulation, optimizationType, fitnessFunction, v => true, maxIterations, crossoverProbability, differentialWeight)
        { }

        #region Optimization params

        /// <summary>Gets the maximum number of iterations allowed to the optimization process</summary>
        public int MaxIterations { get { return _maxIterations; } }

        /// <summary>Gets the current status of the optimization process</summary>
        public SolverStatus Status { get { return _status; } }

        /// <summary>Gets the type of optimization performed (min or max)</summary>
        public OptimizationType OptimizationType { get { return _optimizationType; } }

        #endregion

        #region DE params
        /// <summary>Returns the crossover probability</summary>
        public double CrossoverProbability { get { return _crossoverProbability; } }

        /// <summary>Returns the differential weight</summary>
        public double DifferentialWeight { get { return _differentialWeight; } }

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

        /// <summary>Optimizes the fitness function using Differential Evolution</summary>
        public void Optimize()
        {
            #region Define the general vars
            int sign = _optimizationType == OptimizationType.Min ? -1 : 1;
            Vector[] population = _initialPopulation.Select(v => v.Clone).ToArray();
            double[] fitnesses = population.Select(v => _fitnessFunction(v)).ToArray();
            #endregion

            _convergence.Clear();
            Random rnd = new Random(Guid.NewGuid().GetHashCode());

            EndCriteria endCriteria = new EndCriteria(maxIterations: _maxIterations);
            double fitnessOptimum;
            int optimumIndex = 0;
            while (!endCriteria.ShouldStop())
            {

                for (int k = 0; k < _populationSize; k++)
                {
                    Vector x = population[k];

                    #region Pick neighbours
                    List<int> agentsIndices = Randomizer.PickRandomNumbers(0, _populationSize, 3, new int[] { k }, rnd);
                    Vector a = population[agentsIndices[0]],
                        b = population[agentsIndices[1]],
                        c = population[agentsIndices[2]];
                    #endregion

                    #region Build alternative agent
                    Vector y = Vector.Create(x.Size);
                    do
                    {
                        for (int i = 0; i < x.Size; i++)
                            y[i] = rnd.NextDouble() < _crossoverProbability ?
                                a[i] + _differentialWeight * (b[i] - c[i]) :
                                x[i];
                    }
                    while (!_isFeasable(y));
                    #endregion

                    #region Proceed replacement if relevant
                    double newFitness = _fitnessFunction(y);
                    if ((_optimizationType == OptimizationType.Min && newFitness < fitnesses[k])
                        || (_optimizationType == OptimizationType.Max && newFitness > fitnesses[k]))
                    {
                        population[k] = y;
                        fitnesses[k] = newFitness;
                    }
                    #endregion
                }

                #region Find best and fill convergence serie

                fitnessOptimum = _optimizationType == OptimizationType.Min ? fitnesses.Min() : fitnesses.Max();
                optimumIndex = Array.IndexOf(fitnesses, fitnessOptimum);

                _convergence.Add(new Tuple<Vector, double>(population[optimumIndex].Clone, fitnessOptimum));

                #endregion
            }

            _result = population[optimumIndex].Clone;
            _status = endCriteria.Status;
        }
    }
}
