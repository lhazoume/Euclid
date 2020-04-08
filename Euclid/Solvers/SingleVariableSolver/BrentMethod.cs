using System;
using System.Collections.Generic;

namespace Euclid.Solvers.SingleVariableSolver
{
    /// <summary>Finds a root using the Brent method</summary>
    public class BrentMethod : ISingleVariableSolver
    {
        #region Declarations
        private bool _trackConvergence;
        private double _lowerBound, _upperBound,
            _absoluteTolerance,
            _slopeTolerance,
            _result = 0,
            _error = 0;
        private readonly List<Tuple<double, double>> _convergence = new List<Tuple<double, double>>();
        private Func<double, double> _f;
        private int _maxIterations;
        private SolverStatus _status = SolverStatus.NotRan;
        #endregion

        /// <summary>Builds a solver using the Brent method</summary>
        /// <param name="initialLowerBound">the lower bound of the initial interval</param>
        /// <param name="initialUpperBound">the upper bound of the initial interval</param>
        /// <param name="f">the function to solve for</param>
        /// <param name="maxIterations">the maximum number of iterations</param>
        public BrentMethod(double initialLowerBound, double initialUpperBound,
            Func<double, double> f,
            int maxIterations)
        {
            _trackConvergence = false;
            _lowerBound = initialLowerBound;
            _upperBound = initialUpperBound;
            _f = f;
            _maxIterations = maxIterations;
            _absoluteTolerance = Descents.ERR_EPSILON;
            _slopeTolerance = Descents.GRADIENT_EPSILON;
        }

        #region Accessors

        #region Settables
        /// <summary>Gets and sets the function to solve for</summary>
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
            get => _maxIterations;
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
        public double Result => _result;

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
            _convergence.Clear();

            if (_f == null) throw new NullReferenceException("Brent function should not be null");

            double a = _lowerBound,
                b = _upperBound,
                fa = _f(a) - target,
                fb = _f(b) - target;

            #region Check bracketing
            if (fa * fb >= 0)
            {
                _status = SolverStatus.BadFunction;
                return;
            }
            #endregion

            #region Swap a and b
            if (Math.Abs(fa) < Math.Abs(fb))
            {
                double tmp = a;
                a = b;
                b = tmp;

                tmp = fa;
                fa = fb;
                fb = tmp;
            }
            #endregion

            double c = a,
                fc = fa,
                s = 0, fs,
                d = 0;
            bool mFlag = true;
            _error = double.PositiveInfinity;

            EndCriteria endCriteria = new EndCriteria(maxIterations: _maxIterations);
            while (!endCriteria.ShouldStop(_error))
            {
                if (Math.Abs(b - a) < _absoluteTolerance)
                {
                    _status = SolverStatus.FunctionConvergence;
                    _result = 0.5 * (b + a);
                    _error = _f(_result);
                    return;
                }

                if (fa != fc && fb != fc)
                    s = a * fb * fc / ((fa - fb) * (fa - fc))
                        + b * fa * fc / ((fb - fa) * (fb - fc))
                        + c * fa * fb / ((fc - fa) * (fc - fb));
                else
                    s = b - fb * (b - a) / (fb - fa);

                #region Checks if bisection is relevant or not
                double tmp2 = (3 * a + b) / 4;
                if (!((s > tmp2 && s < b) || (s < tmp2 && s > b)) ||
                    (mFlag && (Math.Abs(s - b) >= 0.5 * Math.Abs(b - c) || Math.Abs(b - c) < _absoluteTolerance)) ||
                    (!mFlag && (Math.Abs(s - b) >= 0.5 * Math.Abs(c - d) || Math.Abs(c - d) < _absoluteTolerance)))
                {
                    s = 0.5 * (a + b);
                    mFlag = true;
                }
                else
                    mFlag = false;
                #endregion

                #region Calculate error
                fs = _f(s) - target;
                if (_trackConvergence)
                    _convergence.Add(new Tuple<double, double>(s, fs));
                #endregion

                d = c;
                c = b;
                fc = fb;

                if (fa * fs < 0)
                {
                    b = s;
                    fb = fs;
                }
                else
                {
                    a = s;
                    fa = fs;
                }

                #region Swap a and b
                if (Math.Abs(fa) < Math.Abs(fb))
                {
                    double tmp = a;
                    a = b;
                    b = tmp;

                    tmp = fa;
                    fa = fb;
                    fb = tmp;
                }
                #endregion
            }

            _status = endCriteria.Status;
            _result = s;
            _error = _f(_result) - target;
        }
        #endregion
    }
}
