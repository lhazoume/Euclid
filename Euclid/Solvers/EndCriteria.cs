using System;
using System.Collections.Generic;
using System.Linq;

namespace Euclid.Solvers
{

    public enum FunctionToleranceMode
    {
        AbsoluteOnly,
        RelativeOnly,
        Both
    }

    /// <summary>
    /// Class used to specify the end criterion or criteria for all iterative optimisation classes
    /// </summary>
    public class EndCriteria
    {
        #region private vars
        private readonly int? _maxIterations, _maxStaticIterations;
        private int _iterations;
        private readonly double? _functionEpsilon, _gradientEpsilon;
        private readonly List<double> _history;
        private readonly FunctionToleranceMode _funcMode;
        private SolverStatus _status;
        #endregion

        /// <summary>End criteria constructor</summary>
        /// <param name="maxIterations">the maximum number of iterations</param>
        /// <param name="maxStaticIterations">the maximum number of iterations without substancial change</param>
        /// <param name="functionEpsilon">the error tolerance (beyond that value, convergence is assumed)</param>
        /// <param name="gradientEpsilon">the gradient tolerance (beyond that value, convergence is assumed)</param>
        public EndCriteria(int? maxIterations = null,int? maxStaticIterations = null,double? functionEpsilon = null,double? gradientEpsilon = null,FunctionToleranceMode mode = FunctionToleranceMode.AbsoluteOnly)
        {
            _iterations = 0;
            _history = new List<double>();
            _status = SolverStatus.NotRan;

            _maxIterations = maxIterations;
            _maxStaticIterations = maxStaticIterations;
            _functionEpsilon = functionEpsilon;
            _gradientEpsilon = gradientEpsilon;
            _funcMode = mode;
        }

        /// <summary>Specifies whether the end criteria are met for the current value</summary>
        /// <param name="value">the current value of the optimized function</param>
        /// <param name="gradient">the current gradient norm of the optimized function</param>
        /// <returns>a boolean</returns>
        public bool ShouldStop(double value, double gradient)
        {
            _iterations++;
            _history.Add(value);
            if (_maxStaticIterations.HasValue && _history.Count > _maxStaticIterations.Value)
            {
                _history.RemoveRange(0, _history.Count - _maxStaticIterations.Value);
            }

            return ExceededIterations() || ExceededMaxStaticIterations() || BelowGradientEpsilon(gradient) || CheckFunctionTolerance(value);
        }
        /// <summary>Specifies whether the end criteria are met for the current value</summary>
        /// <param name="value">the current value of the optimizated function</param>
        /// <returns>a boolean</returns>

        public bool ShouldStop(double value)
        {
            _iterations++;
            _history.Add(value);
            if (_maxStaticIterations.HasValue && _history.Count > _maxStaticIterations.Value)
            {
                _history.RemoveRange(0, _history.Count - _maxStaticIterations.Value);
            }

            return ExceededIterations() || ExceededMaxStaticIterations() || CheckFunctionTolerance(value);
        }

        /// <summary>Specifies whether the end criteria are met for the current value</summary>
        /// <returns>a boolean</returns>
        public bool ShouldStop()
        {
            _iterations++;
            return ExceededIterations();
        }
        
        /// <summary>Gets the current status of the optimization controlled by this end criteria</summary>
        public SolverStatus Status => _status;

        private bool BelowFunctionEpsilon(double value)
        {
            if (!_functionEpsilon.HasValue || Math.Abs(value) >= _functionEpsilon) return false;
            _status = SolverStatus.FunctionConvergence;
            return true;
        }

        private bool BelowFunctionChangeEpsilon()
        {
            if (!_functionEpsilon.HasValue || _history.Count < 2)
                return false;

            // relative change
            if (Math.Abs(_history[_history.Count - 2] - _history[_history.Count - 1]) <= _functionEpsilon.Value * (Math.Abs(_history[_history.Count - 2]) + _functionEpsilon.Value))
            {
                _status = SolverStatus.FunctionConvergence;
                return true;
            }

            return false;
        }
        private bool CheckFunctionTolerance(double value)
        {
            switch (_funcMode)
            {
                case FunctionToleranceMode.AbsoluteOnly:
                    return BelowFunctionEpsilon(value);

                case FunctionToleranceMode.RelativeOnly:
                    return BelowFunctionChangeEpsilon();

                case FunctionToleranceMode.Both:
                    if (BelowFunctionEpsilon(value))
                        return true;
                    return BelowFunctionChangeEpsilon();

                default:
                    return false;
            }
        }
        private bool BelowGradientEpsilon(double gradientValue)
        {
            if (!_gradientEpsilon.HasValue || Math.Abs(gradientValue) >= _gradientEpsilon) return false;
            _status = SolverStatus.GradientConvergence;
            return true;
        }

        private bool ExceededIterations()
        {
            if (!_maxIterations.HasValue || _iterations <= _maxIterations.Value) return false;
            _status = SolverStatus.IterationExceeded;
            return true;
        }

        private bool ExceededMaxStaticIterations()
        {
            if (!_maxStaticIterations.HasValue || _history.Count < _maxStaticIterations.Value || _history.Max() - _history.Min() >= _gradientEpsilon) return false;
            _status = SolverStatus.StationaryFunction;
            return true;
        }
    }
}
