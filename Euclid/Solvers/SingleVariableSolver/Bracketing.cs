using System;
using System.Collections.Generic;

namespace Euclid.Solvers.SingleVariableSolver
{
    /// <summary>Finds a root using root bracketing methods</summary>
    public class Bracketing : ISingleVariableSolver
    {
        #region Declarations
        private bool _trackConvergence;
        private double _lowerBound, _upperBound,
            _tolerance,
            _result = 0,
            _error = 0;
        private readonly List<Tuple<double, double>> _convergence = new List<Tuple<double, double>>();
        private Func<double, double> _f;
        private int _maxIterations;
        private BracketingMethod _method;
        private SolverStatus _status = SolverStatus.NotRan;
        #endregion

        /// <summary>Builds a solver using root bracketing methods</summary>
        /// <param name="initialLowerBound">the lower bound of the initial interval</param>
        /// <param name="initialUpperBound">the upper bound of the initial interval</param>
        /// <param name="f">the function to solve for</param>
        /// <param name="method">the root bracketing method</param>
        /// <param name="maxIterations">the maximum number of iterations</param>
        public Bracketing(double initialLowerBound, double initialUpperBound,
            Func<double, double> f,
            BracketingMethod method,
            int maxIterations)
        {
            _trackConvergence = false;
            _lowerBound = initialLowerBound;
            _upperBound = initialUpperBound;
            _f = f;
            _maxIterations = maxIterations;
            _method = method;
            _tolerance = Descents.ERR_EPSILON;
        }

        #region Accessors

        #region Settables
        /// <summary>Gets and sets the function to solve for </summary>
        public Func<double, double> Function
        {
            get => _f;
            set => _f = value;
        }

        /// <summary>Gets and sets the lower bound of the interval</summary>
        public double LowerBound
        {
            get => _lowerBound;
            set => _lowerBound = value;
        }

        /// <summary>Gets and sets the upper bound of the interval</summary>
        public double UpperBound
        {
            get => _upperBound;
            set => _upperBound = value;
        }

        /// <summary>Gets and sets the maximum number of iterations</summary>
        public int MaxIterations
        {
            get { return _maxIterations; }
            set
            {
                if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value), "The maximum number of iterations should be positive");
                _maxIterations = value;
            }
        }

        /// <summary>Gets and sets the root bracketing method</summary>
        public BracketingMethod Method
        {
            get => _method;
            set => _method = value;
        }

        /// <summary>Gets and sets the tolerance for the target (threshold for target reached)</summary>
        public double Tolerance
        {
            get => _tolerance;
            set => _tolerance = value;
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
        public double Error=> _error;

        /// <summary>The result of the solver</summary>
        public double Result => _result; 

        /// <summary>The final status of the solver</summary>
        public SolverStatus Status=> _status;

        /// <summary>Gets the details of the convergence (value, error) </summary>
        public List<Tuple<double, double>> Convergence
        {
            get { return new List<Tuple<double, double>>(_convergence); }
        }

        #endregion

        #endregion

        #region Methods
        /// <summary>Solve the equation f(x)=0 using the root bracketing method</summary>
        public void Solve()
        {
            Solve(0);
        }

        /// <summary>Solve the equation f(x)=target using the root bracketing method</summary>
        /// <param name="target">the target</param>
        public void Solve(double target)
        {
            _convergence.Clear();

            EndCriteria endCriteria = new EndCriteria(maxIterations: _maxIterations);
            while (!endCriteria.ShouldStop(_error))
            {
                double fu = _f(_upperBound) - target,
                    fl = _f(_lowerBound) - target,
                    m = _method == BracketingMethod.Dichotomy ?
                        0.5 * (_upperBound + _lowerBound) :
                        _upperBound - fu * (_upperBound - _lowerBound) / (fu - fl),
                    _error = _f(m) - target;
                if (_trackConvergence)
                    _convergence.Add(new Tuple<double, double>(m, _error));

                if (fu * fl > 0)
                {
                    _status = SolverStatus.BadFunction;
                    return;
                }


                if (Math.Sign(_error) == 0 || Math.Abs(_upperBound - _lowerBound) < _tolerance)
                {
                    _status = SolverStatus.FunctionConvergence;
                    _result = m;
                    return;
                }

                if (Math.Sign(_error) == Math.Sign(fl))
                    _lowerBound = m;
                else
                    _upperBound = m;
            }
            _status = endCriteria.Status;
            _result = 0.5 * (_upperBound + _lowerBound);
            _error = _f(_result) - target;
        }
        #endregion
    }
}
