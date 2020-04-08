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
        private bool _trackConvergence;
        private double _initialGuess,
            _absoluteTolerance,
            _slopeTolerance,
            _result = 0,
            _error = 0;
        private List<Tuple<double, double>> _convergence = new List<Tuple<double, double>>();
        private Func<double, double> _f, _df;
        private int _maxIterations;
        private SolverStatus _status = SolverStatus.NotRan;
        #endregion

        /// <summary>Builds a solver using the Newton-Raphson method</summary>
        /// <param name="initialGuess">the initial guess</param>
        /// <param name="f">the function to solve for</param>
        /// <param name="df">the derivative of the function to solve for</param>
        /// <param name="maxIterations">the maximum number of iterations</param>
        public NewtonRaphson(double initialGuess,
            Func<double, double> f,
            Func<double, double> df,
            int maxIterations)
        {
            _trackConvergence = false;
            _initialGuess = initialGuess;
            _absoluteTolerance = Descents.ERR_EPSILON;
            _slopeTolerance = Descents.GRADIENT_EPSILON;
            _f = f;
            _df = df;
            _maxIterations = maxIterations;
        }

        /// <summary>Builds a solver using the Newton-Raphson method</summary>
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
        /// <summary>Gets and sets the function to solve for</summary>
        public Func<double, double> Function
        {
            get { return _f; }
            set { _f = value; }
        }

        /// <summary>Gets and sets the initial guess</summary>
        public double InitialGuess
        {
            get { return _initialGuess; }
            set { _initialGuess = value; }
        }

        /// <summary>Gets and sets the maximum number of iterations</summary>
        public int MaxIterations
        {
            get { return _maxIterations; }
            set
            {
                if (value <= 0) throw new ArgumentOutOfRangeException("The maximum number of iterations should be positive");
                _maxIterations = value;
            }
        }

        /// <summary>Gets and sets the tolerance for the target (threshold for target reached)</summary>
        public double AbsoluteTolerance
        {
            get => _absoluteTolerance;
            set => _absoluteTolerance = value;
        }

        /// <summary>Gets and sets the tolerance for the slope (threshold for stationarity)</summary>
        public double SlopeTolerance
        {
            get => _slopeTolerance;
            set => _slopeTolerance = value;
        }

        /// <summary>Gets and sets whether the details of the convergence are tracked</summary>
        public bool TrackConvergence
        {
            get => _trackConvergence;
            set => _trackConvergence = value;
        }
        #endregion

        #region Get

        /// <summary>Returns the final error</summary>
        public double Error => _error;

        /// <summary>Gets The final status of the solver</summary>
        public SolverStatus Status => _status;

        /// <summary> Gets the result of the solver</summary>
        public double Result=> _result; 

        /// <summary>Gets the details of the convergence (value, error)</summary>
        public List<Tuple<double, double>> Convergence
        {
            get { return new List<Tuple<double, double>>(_convergence); }
        }

        #endregion

        #endregion

        #region Methods
        /// <summary>Solves the equation f(x)=0 using the Newton-Raphson method</summary>
        public void Solve()
        {
            Solve(0);
        }

        /// <summary>Solve the equation f(x)=target using the Newton-Raphson method</summary>
        /// <param name="target">the target</param>
        public void Solve(double target)
        {
            if (_f == null) throw new NullReferenceException("Newton-Raphson function should not be null");
            if (_df == null) _df = _f.Differentiate(DifferenceForm.Central, 1);

            _convergence.Clear();

            _result = _initialGuess;
            _status = SolverStatus.Diverged;
            _error = _f(_result) - target;
            if (_trackConvergence)
                _convergence.Add(new Tuple<double, double>(_result, _error));

            double slope = _df(_result);

            EndCriteria endCriteria = new EndCriteria(maxIterations: _maxIterations,
                functionEpsilon: _absoluteTolerance,
                gradientEpsilon: _slopeTolerance);

            while (!endCriteria.ShouldStop(_error, slope))
            {
                _result -= _error / slope;
                _error = _f(_result) - target;
                if (_trackConvergence)
                    _convergence.Add(new Tuple<double, double>(_result, _error));
                slope = _df(_result);
            }

            _status = endCriteria.Status;
        }

        #endregion
    }
}
