using System;
using System.Collections.Generic;

namespace Euclid.Solvers
{
    public class RootBracketing : ISingleVariableSolver
    {
        #region Declarations
        private double _lowerBound, _upperBound,
            _tolerance,
            _result = 0,
            _error = 0;
        private List<Tuple<double, double>> _convergence = new List<Tuple<double, double>>();
        private Func<double, double> _f;
        private int _iterations = 0,
            _maxIterations;
        private RootBracketingMethod _method;
        private SolverStatus _status = SolverStatus.NotRan;
        #endregion

        public RootBracketing(double initialLowerBound, double initialUpperBound,
            Func<double, double> f,
            RootBracketingMethod method,
            int maxIterations)
        {
            _lowerBound = initialLowerBound;
            _upperBound = initialUpperBound;
            _f = f;
            _maxIterations = maxIterations;
            _method = method;
            _tolerance = Descents.ERR_EPSILON;
        }

        #region Accessors

        #region Settables
        public Func<double, double> Function
        {
            get { return _f; }
            set { _f = value; }
        }
        public double LowerBound
        {
            get { return _lowerBound; }
            set { _lowerBound = value; }
        }
        public double UpperBound
        {
            get { return _upperBound; }
            set { _upperBound = value; }
        }
        public int MaxIterations
        {
            get { return _maxIterations; }
            set { _maxIterations = value; }
        }
        public RootBracketingMethod Method
        {
            get { return _method; }
            set { _method = value; }
        }
        public double Tolerance
        {
            get { return _tolerance; }
            set { _tolerance = value; }
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
        public double Result
        {
            get { return _result; }
        }
        public SolverStatus Status
        {
            get { return _status; }
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
            _convergence.Clear();
            _iterations = 1;
            while (_iterations <= _maxIterations)
            {
                double fu = _f(_upperBound) - target,
                    fl = _f(_lowerBound) - target,
                    m = _method == RootBracketingMethod.Dichotomy ?
                        0.5 * (_upperBound + _lowerBound) :
                        _upperBound - fu * (_upperBound - _lowerBound) / (fu - fl),
                    _error = _f(m) - target;
                _convergence.Add(new Tuple<double, double>(m, _error));

                if (fu * fl > 0)
                {
                    _status = SolverStatus.BadFunction;
                    return;
                }


                if (Math.Sign(_error) == 0 || Math.Abs(_upperBound - _lowerBound) < _tolerance)
                {
                    _status = SolverStatus.Normal;
                    _result = m;
                    return;
                }

                if (Math.Sign(_error) == Math.Sign(fl))
                    _lowerBound = m;
                else
                    _upperBound = m;

                _iterations++;
            }
            _status = SolverStatus.IterationExceeded;
            _result = 0.5 * (_upperBound + _lowerBound);
            _error = _f(_result) - target;
        }
        #endregion
    }
}
