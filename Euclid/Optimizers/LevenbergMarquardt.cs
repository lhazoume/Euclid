using Euclid.Numerics;
using Euclid.Solvers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Euclid.Optimizers
{
    /// <summary>
    /// Nonlinear least‑squares optimizer based on the Levenberg–Marquardt algorithm.
    /// This optimizer is specifically designed to minimize the sum of squared residuals in a system of nonlinear equations,
    /// making it highly effective for curve fitting and parameter estimation problems in scientific computing and engineering.
    /// 
    /// The Levenberg–Marquardt algorithm combines the gradient descent method and the Gauss–Newton method to achieve
    /// stable and efficient convergence, particularly near local minima. It interpolates between these two methods
    /// depending on the current distance to the solution.
    /// 
    /// It is important to note that the Levenberg–Marquardt algorithm is fundamentally a *minimization* algorithm.
    /// It cannot be used to *maximize* a function because it is not designed to locate maxima, and it does not 
    /// follow gradients in the direction of increasing function values.
    /// </summary>
    public class LevenbergMarquardt
    {
        #region Declarations

        private Func<Vector, Vector> _residuals;
        private Func<Vector, Matrix> _jacobian;
        private Vector _initialGuess, _bump;
        private readonly int _maxIter, _maxStaticIter;
        private readonly double _gradientThreshold, _functionThreshold;
        private Vector _result;
        private double _error;
        private int _evaluations;
        private SolverStatus _status = SolverStatus.NotRan;
        private readonly List<double> _convergence = new List<double>();
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="LevenbergMarquardt"/> class.
        /// </summary>
        /// <param name="initialGuess">Initial parameter estimate.</param>
        /// <param name="residuals">Function computing the residual vector r(x).</param>
        /// <param name="jacobian">Function computing the Jacobian matrix J(x) of the residuals. If null, finite differences will be used.</param>
        /// <param name="bump">Initial step size for finite differences in the Jacobian. If null, a default value will be used.</param>
        /// <param name="maxIter">Maximum number of iterations.</param>
        /// <param name="maxStaticIter">Maximum number of static iterations allowed without improvement.</param>
        /// <param name="gradientThreshold">Tolerance for the gradient norm. Optimization stops if ||J^T r|| ≤ gradientTolerance.</param>
        /// <param name="functionThreshold">Tolerance for the function value. Optimization stops if ||r|| ≤ functionTolerance.</param>
        public LevenbergMarquardt(
            Vector initialGuess,
            Func<Vector, Vector> residuals,
            Func<Vector, Matrix> jacobian = null,
            Vector bump = null,
            int maxIter = 100,
            int maxStaticIter = 50,
            double gradientThreshold = Descents.GRADIENT_EPSILON,
            double functionThreshold = Descents.ERR_EPSILON
            )
        {
            if (initialGuess == null) throw new ArgumentNullException(nameof(initialGuess));
            _initialGuess = initialGuess.Clone;
            _residuals = residuals ?? throw new ArgumentNullException(nameof(residuals));
            _bump = bump;
            _jacobian = jacobian ?? _residuals.Jacobian(Bump);

            _maxIter = maxIter;
            _maxStaticIter = maxStaticIter;
            _gradientThreshold = gradientThreshold;
            _functionThreshold = functionThreshold;

        }

        #endregion

        #region Accessors
        /// <summary>Gets or sets the initial guess for the parameters.</summary>
        public Vector InitialGuess
        {
            get { return _initialGuess; }
            set { _initialGuess = value ?? throw new ArgumentNullException(nameof(value)); }
        }
        /// <summary> Gets or sets the bump vector used for finite differences in the Jacobian computation./// </summary>
        public Vector Bump
        {
            get { return _bump ?? Vector.Create(InitialGuess.Size, Descents.STEP_EPSILON); }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));
                if (value.Data.Any(d => d <= 0))
                    throw new ArgumentException("Bump must be strictly positive", nameof(value));
                _bump = value;
            }
        }
        /// <summary> Gets or sets the function that computes the Jacobian matrix of the residuals./// </summary>
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
        /// <summary> Gets or sets the function that computes the residuals vector./// </summary>
        public Func<Vector, Vector> Residuals
        {
            get => _residuals;
            set => _residuals = value ?? throw new ArgumentNullException(nameof(value));
        }
        /// <summary> Gets the current status of the optimizer. </summary>
        public SolverStatus Status => _status;
        /// <summary> Gets the current result of the optimization. </summary>
        public Vector Result => _result;
        /// <summary> Gets the current error value. </summary>
        public double Error => _error;
        /// <summary>Gets the list of error values during the optimization process.</summary>
        public IEnumerable<double> Errors => _convergence;
        /// <summary>Gets the number of evaluations performed during the optimization.</summary>
        public int Evaluations => _evaluations;
        #endregion

        #region Methods
        /// <summary>
        /// Performs the optimization using the Levenberg‑Marquardt algorithm with a fixed tau and penaltyFactor.
        /// </summary>
        /// <param name="tau"></param>
        /// <param name="penaltyFactor"></param>
        public void Optimize(double tau = 1e-3, double penaltyFactor = 2.0)
        {
            double lambda = tau;
            int dimension = _initialGuess.Size;

            _evaluations = 0;
            _convergence.Clear();

            _result = _initialGuess.Clone;
            Vector residual = _residuals(_result);
            _error = residual.SumOfSquares;
            _convergence.Add(_error);
            _evaluations++;

            _status = SolverStatus.Diverged;

            #region Estimation of the initial direction of descent
            Matrix jacobian = _jacobian(_result);
            Vector gradient = jacobian.Transpose * residual;
            Matrix A = Matrix.TransposeBySelf(jacobian) + Matrix.CreateIdentityMatrix(dimension, dimension) * lambda;
            Vector delta = -A.SolveWith(gradient);
            #endregion

            EndCriteria endCriteria = new EndCriteria(maxIterations: _maxIter,maxStaticIterations: _maxStaticIter,functionEpsilon: _functionThreshold,gradientEpsilon: _gradientThreshold,FunctionToleranceMode.RelativeOnly);

            while (!endCriteria.ShouldStop(value: _error, gradient: gradient.Norm2))
            {
                lambda = OptimalLambda(_error, _result, delta, lambda, penaltyFactor);

                A = Matrix.TransposeBySelf(jacobian) + Matrix.CreateIdentityMatrix(dimension, dimension) * lambda;
                delta = -A.SolveWith(gradient);

                _result += delta;
                residual = _residuals(_result);
                _evaluations++;
                _error = residual.SumOfSquares;
                _convergence.Add(_error);

                #region  Update of the direction of descent
                jacobian = _jacobian(_result);
                gradient = jacobian.Transpose * residual;
                #endregion
            }
            _status = endCriteria.Status;
        }
        /// <summary>
        /// Computes the updated lambda value for the Levenberg-Marquardt algorithm based on the current error and proposed step.
        /// If the new solution reduces the error, lambda is decreased (making the step closer to Gauss-Newton).
        /// Otherwise, lambda is increased (making the step closer to gradient descent).
        /// </summary>
        /// <param name="error">The current error (sum of squared residuals) at the current solution.</param>
        /// <param name="solution">The current solution vector.</param>
        /// <param name="delta">The proposed step vector.</param>
        /// <param name="lambda">The current value of the damping parameter lambda.</param>
        /// <param name="penaltyFactor">The factor by which lambda is increased or decreased.</param>
        /// <returns>
        /// The updated value of lambda to be used in the next iteration.
        /// </returns>
        private double OptimalLambda(double error, Vector solution, Vector delta, double lambda, double penaltyFactor)
        {
            if (_residuals(solution + delta).SumOfSquares < error)
                return lambda / penaltyFactor;
            else
                return lambda * penaltyFactor;
        }
        /// <summary>
        /// Performs the optimization using the Levenberg‑Marquardt algorithm with adaptive lambda adjustment based on METHODS FOR NONLINEAR LEAST SQUARES PROBLEMS by  K.Madsen,H.B.Nielsen,O.Tingleff
        /// </summary>
        /// <param name="tau"> tau is a small positive number used to scale the initial damping factor.</param>
        /// <param name="initialPenaltyFactor"> Initial value of the penalty factor.</param>
        public void OptimizeAdaptive(double tau = 1e-3, double initialPenaltyFactor = 2.0)
        {
            int dimension = _initialGuess.Size;
            double penaltyFactor = initialPenaltyFactor;
            _evaluations = 0;
            _convergence.Clear();

            _result = _initialGuess.Clone;
            Vector residual = _residuals(_result);
            _error = residual.SumOfSquares;
            _convergence.Add(_error);
            _evaluations++;

            _status = SolverStatus.Diverged;

            #region Estimation of the initial direction of descent
            Matrix jacobian = _jacobian(_result);
            Vector gradient = jacobian.Transpose * residual;
            Matrix JTJ = Matrix.TransposeBySelf(jacobian);
            double lambda = tau * JTJ.Rows > 0 ? Enumerable.Range(0, JTJ.Rows).Max(i => Math.Abs(JTJ[i, i])) : 1.0; // lambda is initialized to a small positive value based on the maximum diagonal element
            Matrix A = Matrix.TransposeBySelf(jacobian) + Matrix.CreateIdentityMatrix(dimension, dimension) * lambda;
            Vector delta = -A.SolveWith(gradient);
            #endregion

            EndCriteria endCriteria = new EndCriteria(maxIterations: _maxIter, maxStaticIterations: _maxStaticIter, functionEpsilon: _functionThreshold, gradientEpsilon: _gradientThreshold, FunctionToleranceMode.RelativeOnly);
            while (!endCriteria.ShouldStop(value: _error, gradient: gradient.Norm2))
            {
                (lambda, penaltyFactor) = OptimalLambdaAdaptive(_error, _result, delta, gradient, lambda, penaltyFactor, initialPenaltyFactor);
                A = Matrix.TransposeBySelf(jacobian) + Matrix.CreateIdentityMatrix(dimension, dimension) * lambda;
                delta = - A.SolveWith(gradient);

                _result += delta;
                residual = _residuals(_result);
                _evaluations++;
                _error = residual.SumOfSquares;
                _convergence.Add(_error);

                #region Update of the direction of descent
                jacobian = _jacobian(_result);
                gradient = jacobian.Transpose * residual;
                #endregion
            }
            _status = endCriteria.Status;
        }
        /// <summary>
        /// Computes the optimal lambda and penalty factor for the Levenberg-Marquardt algorithm with adaptive adjustment.
        /// </summary>
        /// <param name="error">Current error (||r||^2).</param>
        /// <param name="solution">Current solution.</param>
        /// <param name="delta">Proposed step.</param>
        /// <param name="gradient">Current gradient.</param>
        /// <param name="lambda">Current value of lambda.</param>
        /// <param name="penaltyFactor">Current penalty factor.</param>
        /// <param name="initialPenaltyFactor">Initial value of the penalty factor.</param>
        /// <returns>Updated tuple (lambda, penaltyFactor).</returns>
        private (double, double) OptimalLambdaAdaptive(double error,Vector solution, Vector delta,Vector gradient,double lambda,double penaltyFactor,double initialPenaltyFactor)
        {
            // Verification of the step using a predicted reduction ratio where the value indicates how much the error is expected to decrease.
            double predictedReduction = Vector.Scalar(delta, lambda * delta - gradient);
            double actualReduction = error - _residuals(solution + delta).SumOfSquares;

            double lambdaOutput;
            double penaltyFactorOutput;

            if (predictedReduction > 0.0 && actualReduction > 0.0)
            {
                double reductionRatio = actualReduction / predictedReduction; // closer to 1.0 means better step acceptance
                lambdaOutput = lambda * Math.Max(1.0 / 3.0, 1.0 - Math.Pow(2.0 * reductionRatio - 1.0, 3.0));
                penaltyFactorOutput = initialPenaltyFactor;
            }
            else
            {
                lambdaOutput = lambda * penaltyFactor;
                penaltyFactorOutput = penaltyFactor * 2.0;
            }

            return (lambdaOutput, penaltyFactorOutput);
        }

        #endregion
    }
}
