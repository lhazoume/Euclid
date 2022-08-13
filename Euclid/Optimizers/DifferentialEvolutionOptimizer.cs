using Euclid.Extensions;
using Euclid.Solvers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Euclid.Optimizers
{
    /// <summary>Optimizes a function on a space using Differential Evolution</summary>
    public class DifferentialEvolutionOptimizer
    {
        #region Variables
        private readonly Vector[] _initialPopulation;
        private readonly int _populationSize, _maxIterations;
        private readonly OptimizationType _optimizationType;
        private readonly Func<Vector, double> _fitnessFunction;
        private readonly Func<Vector, bool> _isFeasible;
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
                throw new ArgumentException("The initial population is too small to perform a differential evolution", nameof(initialPopulation));

            if (initialPopulation.Any(v => !feasabilityFunction(v)))
                throw new ArgumentOutOfRangeException(nameof(initialPopulation), "Some agents of the initial population are not feasible");
            _initialPopulation = initialPopulation.ToArray();
            _populationSize = _initialPopulation.Length;

            _isFeasible = feasabilityFunction;
            _fitnessFunction = fitnessFunction;

            _optimizationType = optimizationType;

            if (maxIterations <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxIterations), "The maximum number of iterations and static iterations should both be >0");
            _maxIterations = maxIterations;


            if (crossoverProbability >= 1 || crossoverProbability <= 0)
                throw new ArgumentOutOfRangeException(nameof(crossoverProbability), "The crossover probability should be positive and below 1");
            _crossoverProbability = crossoverProbability;

            if (differentialWeight == 0)
                throw new ArgumentOutOfRangeException(nameof(differentialWeight), "The differential weight should not be equal to zero");

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
        public int MaxIterations => _maxIterations;

        /// <summary>Gets the current status of the optimization process</summary>
        public SolverStatus Status => _status;

        /// <summary>Gets the type of optimization performed (min or max)</summary>
        public OptimizationType OptimizationType => _optimizationType;

        #endregion

        #region DE params
        /// <summary>Returns the crossover probability</summary>
        public double CrossoverProbability => _crossoverProbability;

        /// <summary>Returns the differential weight</summary>
        public double DifferentialWeight => _differentialWeight;

        /// <summary>The result of the solver</summary>
        public Vector Result => _result;

        /// <summary>Gets the details of the convergence (Vector, error)</summary>
        public IEnumerable<Tuple<Vector, double>> Convergence => _convergence;
        #endregion

        /// <summary>Optimizes the fitness function using Differential Evolution</summary>
        public void Optimize(bool isParallel)
        {
            #region Define the general vars
            int sign = _optimizationType == OptimizationType.Min ? -1 : 1;
            Vector[] population = _initialPopulation.Select(v => v.Clone).ToArray();
            double[] fitnesses = population.Select(v => _fitnessFunction(v)).ToArray();
            #endregion

            _convergence.Clear();

            EndCriteria endCriteria = new EndCriteria(maxIterations: _maxIterations);
            double fitnessOptimum;
            int optimumIndex = 0;

            Random rnd = new Random(Guid.NewGuid().GetHashCode());
            while (!endCriteria.ShouldStop())
            {
                if (isParallel)
                {
                    Vector[] newPopulation = new Vector[_populationSize];

                    int[] seeds = Enumerable.Range(0, _populationSize).Select(i => rnd.Next()).ToArray();
                    Parallel.For(0, _populationSize, k =>
                    {
                        Random localRnd = new Random(seeds[k]);
                        Vector x = population[k];

                        #region Pick neighbours
                        List<int> agentsIndices = Randomizer.PickRandomNumbers(0, _populationSize, 3, new int[] { k }, localRnd);
                        Vector a = population[agentsIndices[0]],
                            b = population[agentsIndices[1]],
                            c = population[agentsIndices[2]];
                        #endregion

                        #region Build alternative agent
                        Vector y = Vector.Create(x.Size);
                        do
                        {
                            for (int i = 0; i < x.Size; i++)
                                y[i] = localRnd.NextDouble() < _crossoverProbability ?
                                    a[i] + _differentialWeight * (b[i] - c[i]) :
                                    x[i];
                        }
                        while (!_isFeasible(y));
                        #endregion

                        #region Proceed replacement if relevant
                        double newFitness = _fitnessFunction(y);
                        if ((_optimizationType == OptimizationType.Min && newFitness < fitnesses[k])
                            || (_optimizationType == OptimizationType.Max && newFitness > fitnesses[k]))
                        {
                            newPopulation[k] = y;
                            fitnesses[k] = newFitness;
                        }
                        else
                            newPopulation[k] = x;
                        #endregion
                    });

                    population = newPopulation;
                }
                else
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
                        while (!_isFeasible(y));
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
