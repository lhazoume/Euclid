using Euclid.Numerics;
using Euclid.Solvers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Euclid.Optimizers
{
    /// <summary>
    /// Nonlinear least‑squares optimizer based on the Levenberg–Marquardt algorithm.
    /// </summary>
    public class LevenbergMarquardt
    {
        #region Declarations
        private  Func<Vector, Vector> _residuals;
        private Func<Vector, Matrix> _jacobian;
        private Vector _initialGuess, _bump;
        private readonly int _maxIter, _maxStaticIter;
        private readonly double _gradientThreshold, _functionThreshold;
        private Vector _result;
        private double _error;
        private readonly int _sign;
        private readonly OptimizationType _optimizationType;
        private SolverStatus _status = SolverStatus.NotRan;
        private readonly List<double> _errors = new List<double>();
        private readonly List<double> _lambdas = new List<double>();
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="LevenbergMarquardt"/> class.
        /// </summary>
        /// <param name="initialGuess">Initial parameter estimate.</param>
        /// <param name="residuals">Function computing the residual vector r(x).</param>
        /// <param name="jacobian">Function computing the Jacobian matrix J(x) of the residuals. If null, finite differences will be used.</param>
        /// <param name="bump">Initial step size for finite differences in the Jacobian. If null, a default value will be used.</param>
        /// <param name="optimizationType">Optimization type (minimization or maximization).</param>
        /// <param name="maxIter">Maximum number of iterations.</param>
        /// <param name="maxStaticIter">Maximum number of static iterations allowed without improvement.</param>
        /// <param name="gradientThreshold">Tolerance for the gradient norm. Optimization stops if ||J^T r|| ≤ gradientTolerance.</param>
        /// <param name="functionThreshold">Tolerance for the function value. Optimization stops if ||r|| ≤ functionTolerance.</param>
        public LevenbergMarquardt(
            Vector initialGuess,
            Func<Vector, Vector> residuals,
            Func<Vector, Matrix> jacobian = null,
            Vector bump = null,
            OptimizationType optimizationType = OptimizationType.Min,
            int maxIter = 100,
            int maxStaticIter = 50,
            double gradientThreshold = Descents.GRADIENT_EPSILON,
            double functionThreshold = Descents.ERR_EPSILON
            )
        {
            _initialGuess = initialGuess.Clone;
            _residuals = residuals ?? throw new ArgumentNullException(nameof(residuals));
            _bump = bump;

            _jacobian = jacobian ?? _residuals.Jacobian(Bump);

            _optimizationType = optimizationType;
            _sign = _optimizationType == OptimizationType.Min ? -1 : 1;   

            _maxIter = maxIter;
            _maxStaticIter = maxStaticIter;
            _gradientThreshold = gradientThreshold;
            _functionThreshold = functionThreshold;
        }



        #endregion

        #region Accessors
        /// <summary>Gets or sets the initial guess for the parameters.</summary>
        /// 
        public Vector InitialGuess
        {
            get { return _initialGuess; }
            set { _initialGuess = value ?? throw new ArgumentNullException(nameof(value)); }
        }
        /// <summary>
        /// Gets or sets the bump vector used for finite differences in the Jacobian computation.
        /// </summary>
        public Vector Bump
        {
            get
            {
                return _bump
                    ?? Vector.Create(InitialGuess.Size, Descents.STEP_EPSILON);
            }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));
                if (value.Data.Any(d => d <= 0))
                    throw new ArgumentException("Bump must be strictly positive", nameof(value));
                _bump = value;
            }
        }
        /// <summary>
        /// Gets or sets the function that computes the Jacobian matrix of the residuals.
        /// </summary>
        public Func<Vector, Matrix> Jacobian
        {
            get
            {
                if (_jacobian == null) 
                    _jacobian = _residuals.Jacobian(Bump);
                return _jacobian;
            }
            set => _jacobian = value;
        }
        public Func<Vector, Vector> Residuals
        {
            get => _residuals;
            set => _residuals = value ?? throw new ArgumentNullException(nameof(value));
        }
        /// <summary>
        /// Gets the current status of the optimizer.
        /// </summary>
        public SolverStatus Status => _status;
        /// <summary>
        /// Gets the current result of the optimization.
        /// </summary>
        public Vector Result => _result;
        /// <summary>
        /// Gets the current error value.
        /// </summary>
        public double Error => _error;
        /// <summary>
        /// Gets the list of error values during the optimization process.
        /// </summary>
        public IEnumerable<double> Errors => _errors;
        /// <summary>
        /// Gets the list of lambda values used during the optimization process.
        /// </summary>
        public IEnumerable<double> Lambdas => _lambdas;
        /// <summary>Gets the optimization type</summary>
        public OptimizationType OptimizationType => _optimizationType;

        #endregion
        #region Methods
        /// <summary>
        /// Performs the optimization using the Levenberg‑Marquardt algorithm with adaptive lambda adjustment based on METHODS FOR NONLINEAR LEAST SQUARES PROBLEMS by  K.Madsen,H.B.Nielsen,O.Tingleff
        /// </summary>
        /// <param name="tau"></param>
        /// <param name="vInit"></param>
        public void OptimizeAdaptive(double tau = 1e-3, double vInit = 2.0)
        {
            _result = _initialGuess.Clone;
            _errors.Clear();
            _lambdas.Clear();
            _status = SolverStatus.Diverged;

            EndCriteria endCriteria = new EndCriteria(maxIterations: _maxIter,maxStaticIterations: _maxStaticIter,functionEpsilon: _functionThreshold,gradientEpsilon: _gradientThreshold);

            double lambda = 0;
            double v = vInit;
            int dimension = _result.Size;
            while (true)
            {
                Vector residual = _residuals(_result);
                _error = 0.5 * residual.SumOfSquares;
                Matrix jacobian = _jacobian(_result);
                Vector gradient = jacobian.Transpose * residual;
                Matrix JTJ = Matrix.TransposeBySelf(jacobian);
                if (_errors.Count == 0)
                {
                    double diagMax = JTJ.Rows > 0 ? Enumerable.Range(0, JTJ.Rows).Max(i => Math.Abs(JTJ[i, i])) : 1.0;
                    lambda = tau * diagMax;
                }

                double gradNorm = gradient.Norm2;
                Matrix A = JTJ + Matrix.CreateIdentityMatrix(dimension,dimension) * lambda;
                Vector delta = _sign * A.SolveWith(gradient);

                Vector resultNew = _result + delta;
                Vector residualNew = _residuals(resultNew);
                double errorNew = 0.5 * residualNew.SumOfSquares;

                double predictedReduction = -_sign * 0.5 * Vector.Scalar(delta, lambda * delta - gradient);
                double actualReduction = -_sign * (_error - errorNew);
                double reductionRatio = (predictedReduction > double.Epsilon) ? actualReduction / predictedReduction : -1.0;

                if (reductionRatio > 0)
                {
                    _result = resultNew;
                    _error = errorNew;
                    lambda *= Math.Max(1.0 / 3.0, 1.0 - Math.Pow(2.0 * reductionRatio - 1.0, 3.0));
                    v = vInit;
                    if (endCriteria.ShouldStop(value: _error, gradient: gradNorm))
                    {
                        _status = endCriteria.Status;
                        return;
                    }
                }
                else
                {
                    lambda *= v;
                    v *= 2.0;
                }
                _errors.Add(_error);
                _lambdas.Add(lambda);
            }
        }
        /// <summary>
        /// Performs the optimization using the Levenberg‑Marquardt algorithm with a fixed tau and vInit.
        /// </summary>
        /// <param name="tau"></param>
        /// <param name="vInit"></param>
        public void Optimize(double tau = 1e-3, double vInit = 2.0)
        {
            _result = _initialGuess.Clone;
            _errors.Clear();
            _lambdas.Clear();
            _status = SolverStatus.Diverged;

            EndCriteria endCriteria = new EndCriteria(maxIterations: _maxIter,maxStaticIterations: _maxStaticIter,functionEpsilon: _functionThreshold,gradientEpsilon: _gradientThreshold);

            double lambda = tau;
            double v = vInit;
            int dimension = _result.Size;
            while (true)
            {
                Vector residual = _residuals(_result);
                _error = 0.5 * residual.SumOfSquares;
                Matrix jacobian = _jacobian(_result);
                Vector gradient = jacobian.Transpose * residual;
               
                Matrix A = Matrix.TransposeBySelf(jacobian) + Matrix.CreateIdentityMatrix(dimension,dimension) * lambda;
                Vector delta = _sign * A.SolveWith(gradient);
                double gradNorm = gradient.Norm2;

                Vector resultNew = _result + delta;
                Vector residualNew = _residuals(resultNew);
                double errorNew = 0.5 * residualNew.SumOfSquares;

                if (errorNew - _error < 0)
                {
                    _result = resultNew;
                    _error = errorNew;
                    lambda /= v;

                    if (endCriteria.ShouldStop(value: _error, gradient: gradNorm))
                    {
                        _status = endCriteria.Status;
                        return;
                    }
                }
                else
                {
                    lambda *= v;
                }

                _errors.Add(_error);
                _lambdas.Add(lambda);
            }
        }

        #endregion
    }
}
