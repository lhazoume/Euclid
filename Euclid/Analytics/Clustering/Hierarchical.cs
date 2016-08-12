using Euclid.IndexedSeries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Euclid.Analytics.Clustering
{
    public class Hierarchical<T, V> where T : IComparable<T>, IEquatable<T> where V : IEquatable<V>, IConvertible
    {
        private class Cluster
        {
            private List<int> _indices = new List<int>();

            public Cluster(IEnumerable<int> indices)
            {
                _indices.AddRange(indices);
            }

            public int Count
            {
                get { return _indices.Count; }
            }
            public List<int> Indices
            {
                get { return _indices; }
            }
        }

        private List<Vector> _predictors = new List<Vector>();
        private Vector _predicted;
        private List<Cluster> _clusters = new List<Cluster>();
        private double _totalAverage, _totalVariance;

        public Hierarchical(DataFrame<T, double, V> data, Series<T, double, V> output)
        {
            Slice<T, double, V>[] rows = data.GetRows();
            for (int i = 0; i < rows.Length; i++)
                _predictors.Add(Vector.Create(rows[i].Data));
            _predicted = Vector.Create(output.Data);
            int n = _predicted.Size;
            _totalAverage = _predicted.Sum / n;
            _totalVariance = Math.Pow(_totalAverage, 2) - _predicted.SumOfSquares / n;
        }

        public void PerformOneStep()
        {
            int i0 = 0,
                j0 = 1;
            double minDistance = double.MaxValue;

            for (int i = 0; i < _clusters.Count - 1; i++)
                for (int j = i + 1; j < _clusters.Count; j++)
                {
                    Cluster ci = _clusters[i],
                        cj = _clusters[j];

                    double dissimilarity = 0;
                    #region Calculate the dissimilarity between the clusters
                    for (int k = 0; k< ci.Count;k++)
                        for(int p = 0; p < cj.Count; p++)
                        {
                            double localDistance = (_predictors[ci.Indices[k]] - _predictors[cj.Indices[p]]).Norm2;
                            if (dissimilarity < localDistance) dissimilarity = localDistance;
                        }
                    #endregion

                    if(dissimilarity <minDistance)
                    {
                        i0 = i;
                        j0 = j;
                        minDistance = dissimilarity;
                    }
                }
            _clusters[i0].Indices.AddRange(_clusters[j0].Indices);
            _clusters.RemoveAt(j0);
        }

        public double TotalVariance
        {
            get { return _totalVariance; }
        }

        public double BetweenGroupsVariance
        {
            get
            {
                double result = 0;
                for (int i = 0; i < _clusters.Count; i++)
                {
                    int count = _clusters[i].Count;
                    double localSum = 0;
                    for (int j = 0; j < count; j++)
                        localSum += _predicted[_clusters[i].Indices[j]];
                    localSum /= count;
                    result += count * Math.Pow(localSum - _totalAverage, 2);
                }
                return result;
            }
        }
    }
    /*
    public class NewtonRaphson : ISingleVariableSolver
    {
        #region Declarations

        private double _initialGuess,
            _absoluteTolerance,
            _slopeTolerance,
            _result = 0,
            _error = 0;
        private List<Tuple<double, double>> _convergence = new List<Tuple<double, double>>();
        private Func<double, double> _f, _df;
        private int _iterations = 0,
            _maxIterations;
        private SolverStatus _status = SolverStatus.NotRan;

        #endregion

        /// <summary>
        /// Builds a solver using the Newton-Raphson method
        /// </summary>
        /// <param name="initialGuess">the initial guess</param>
        /// <param name="f">the function to solve for</param>
        /// <param name="df">the derivative of the function to solve for</param>
        /// <param name="maxIterations">the maximum number of iterations</param>
        public NewtonRaphson(double initialGuess,
            Func<double, double> f,
            Func<double, double> df,
            int maxIterations)
        {
            _initialGuess = initialGuess;
            _absoluteTolerance = Descents.ERR_EPSILON;
            _slopeTolerance = Descents.GRADIENT_EPSILON;
            _f = f;
            _df = df;
            _maxIterations = maxIterations;
        }

        /// <summary>
        /// Builds a solver using the Newton-Raphson method
        /// </summary>
        /// <param name="initialGuess">the initial guess</param>
        /// <param name="f">the function to solve for</param>
        /// <param name="maxIterations">the maximum number of iterations</param>
        public NewtonRaphson(double initialGuess,
            Func<double, double> f,
            int maxIterations)
            : this(initialGuess, f, f.Differentiate(), maxIterations)
        { }

        #region Accessors

        #region Settables
        /// <summary>
        /// Gets and sets the function to solve for
        /// </summary>
        public Func<double, double> Function
        {
            get { return _f; }
            set { _f = value; }
        }

        /// <summary>
        /// Gets and sets the initial guess
        /// </summary>
        public double InitialGuess
        {
            get { return _initialGuess; }
            set { _initialGuess = value; }
        }

        /// <summary>
        /// Gets and sets the maximum number of iterations
        /// </summary>
        public int MaxIterations
        {
            get { return _maxIterations; }
            set
            {
                if (value <= 0) throw new ArgumentOutOfRangeException("The maximum number of iterations should be positive");
                _maxIterations = value;
            }
        }

        /// <summary>
        /// Gets and sets the tolerance for the target (threshold for target reached)
        /// </summary>
        public double AbsoluteTolerance
        {
            get { return _absoluteTolerance; }
            set { _absoluteTolerance = value; }
        }

        /// <summary>
        /// Gets and sets the tolerance for the slope (threshold for stationarity)
        /// </summary>
        public double SlopeTolerance
        {
            get { return _slopeTolerance; }
            set { _slopeTolerance = value; }
        }

        #endregion

        #region Get

        /// <summary>
        /// Returns the final error
        /// </summary>
        public double Error
        {
            get { return _error; }
        }

        /// <summary>
        /// Returns the number of interations of the solver
        /// </summary>
        public int Iterations
        {
            get { return _iterations; }
        }

        /// <summary>
        /// The final status of the solver
        /// </summary>
        public SolverStatus Status
        {
            get { return _status; }
        }

        /// <summary>
        /// The result of the solver
        /// </summary>
        public double Result
        {
            get { return _result; }
        }

        /// <summary>
        /// The details of the convergence (value, error)
        /// </summary>
        public List<Tuple<double, double>> Convergence
        {
            get { return new List<Tuple<double, double>>(_convergence); }
        }

        #endregion

        #endregion

        #region Methods
        /// <summary>
        /// Solve the equation f(x)=0 using the Newton-Raphson method
        /// </summary>
        public void Solve()
        {
            Solve(0);
        }

        /// <summary>
        /// Solve the equation f(x)=target using the Newton-Raphson method
        /// </summary>
        /// <param name="target">the target</param>
        public void Solve(double target)
        {
            if (_f == null) throw new NullReferenceException("Newton-Raphson function should not be null");
            if (_df == null) _df = _f.Differentiate(DifferenceForm.Central, 1);

            _convergence.Clear();

            _result = _initialGuess;
            _status = SolverStatus.Diverged;
            _error = _f(_result) - target;
            _convergence.Add(new Tuple<double, double>(_result, _error));

            double slope = _df(_result);

            _iterations = 1;

            while (Math.Abs(_error) > _absoluteTolerance && Math.Abs(slope) > _slopeTolerance && _iterations <= _maxIterations)
            {
                _result = _result - _error / slope;
                _error = _f(_result) - target;
                _convergence.Add(new Tuple<double, double>(_result, _error));
                slope = _df(_result);
                _iterations++;
            }

            if (Math.Abs(_error) <= _absoluteTolerance)
                _status = SolverStatus.Normal;
            else if (_iterations > _maxIterations)
                _status = SolverStatus.IterationExceeded;
            else if (Math.Abs(slope) <= _slopeTolerance)
                _status = SolverStatus.BadFunction;
        }

        #endregion
    }*/
}
