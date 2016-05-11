using Euclid.Numerics;
using System;
using System.Collections.Generic;

namespace Euclid.Solvers
{
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

        public NewtonRaphson(double initialGuess,
            Func<double, double> f,
            int maxIterations)
            : this(initialGuess, f, f.Differentiate(), maxIterations)
        { }

        #region Accessors

        #region Settables

        public Func<double, double> Function
        {
            get { return _f; }
            set { _f = value; }
        }
        public double InitialGuess
        {
            get { return _initialGuess; }
            set { _initialGuess = value; }
        }
        public int MaxIterations
        {
            get { return _maxIterations; }
            set { _maxIterations = value; }
        }
        public double AbsoluteTolerance
        {
            get { return _absoluteTolerance; }
            set { _absoluteTolerance = value; }
        }
        public double SlopeTolerance
        {
            get { return _slopeTolerance; }
            set { _slopeTolerance = value; }
        }

        #endregion

        #region Get

        public double Error
        {
            get { return _error; }
        }
        public int Iterations
        {
            get { return _iterations; }
        }
        public SolverStatus Status
        {
            get { return _status; }
        }
        public double Result
        {
            get { return _result; }
        }
        public List<Tuple<double, double>> Convergence
        {
            get { return new List<Tuple<double, double>>(_convergence); }
        }

        #endregion

        #endregion

        #region Methods

        public void Solve()
        {
            Solve(0);
        }
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
