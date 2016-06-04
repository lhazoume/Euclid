using Euclid.Numerics;
using System;
using System.Collections.Generic;

namespace Euclid.Solvers
{
    /// <summary>
    /// Finds a root using the Newton-Raphson method
    /// </summary>
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
            if (_df == null) _df = _f.Differentiate();

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
    }
}
