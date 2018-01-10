using System;
using System.Collections.Generic;
using System.Linq;

namespace Euclid.Solvers
{
    /// <summary>
    /// Class used to perform a gradient descent on any multivariate function
    /// </summary>
    public class GradientDescent
    {
        #region Declarations
        private Func<Vector, double> _function;
        private Func<Vector, Vector> _gradient;
        private double _error, _gradientThreshold;
        private int _maxIterations, _maxLineSearchIterations, _evaluations;
        private Vector _initialGuess, _result;
        private List<Vector> _descentDirections = new List<Vector>();
        private List<Tuple<double, double>> _convergence = new List<Tuple<double, double>>();
        private LineSearch _lineSearch;
        private OptimizationType _optimizationType;
        private int _sign;
        private SolverStatus _status = SolverStatus.NotRan;
        #endregion

        /// <summary>Builds a GradientDescent helper</summary>
        /// <param name="initialGuess">the initial guess Vector</param>
        /// <param name="lineSearch">the line search method</param>
        /// <param name="function">the function to minimize</param>
        /// <param name="optimizationType">the optimization type</param>
        /// <param name="maxIterations">the maximum number of iterations in the gradient</param>
        /// <param name="maxLineSearchIterations">the maximum number of iterations in the line search</param>
        /// <param name="gradientNormThreshold">the threshold for the gradient norm</param>
        public GradientDescent(Vector initialGuess, LineSearch lineSearch,
            Func<Vector, double> function, OptimizationType optimizationType,
            int maxIterations, int maxLineSearchIterations, double gradientNormThreshold = Descents.GRADIENT_EPSILON)
        {
            _initialGuess = initialGuess.Clone;
            _lineSearch = lineSearch;
            _maxIterations = maxIterations;
            _evaluations = 0;
            _function = function;
            _maxLineSearchIterations = maxLineSearchIterations;
            _optimizationType = optimizationType;
            _sign = _optimizationType == OptimizationType.Min ? -1 : 1;
            _gradientThreshold = gradientNormThreshold;
            _gradient = x => NumericalGradient(x, Vector.Create(_initialGuess.Size, Descents.STEP_EPSILON));
        }

        /// <summary>Gradient descent constructor</summary>
        /// <param name="initialGuess">the initial guess Vector</param>
        /// <param name="lineSearch">the line search method</param>
        /// <param name="function">the function to minimize</param>
        /// <param name="gradient">the gradient function to minimize</param>
        /// <param name="optimizationType">the optimization type</param>
        /// <param name="maxIterations">the maximum number of iterations in the gradient</param>
        /// <param name="maxLineSearchIterations">the maximum number of iterations in the line search</param>
        /// <param name="gradientNormThreshold">the threshold for the gradient norm</param>
        public GradientDescent(Vector initialGuess, LineSearch lineSearch,
            Func<Vector, double> function, Func<Vector, Vector> gradient, OptimizationType optimizationType,
            int maxIterations, int maxLineSearchIterations, double gradientNormThreshold = Descents.GRADIENT_EPSILON)
        {
            _initialGuess = initialGuess.Clone;
            _lineSearch = lineSearch;
            _maxIterations = maxIterations;
            _evaluations = 0;
            _function = function;
            _maxLineSearchIterations = maxLineSearchIterations;
            _optimizationType = optimizationType;
            _gradientThreshold = gradientNormThreshold;
            _sign = _optimizationType == OptimizationType.Min ? -1 : 1;

            _gradient = gradient;
        }

        #region Accessors

        #region Settables

        /// <summary>Gets and sets the line search method used to optimize the step</summary>
        public LineSearch LineSearch
        {
            get { return _lineSearch; }
            set { _lineSearch = value; }
        }

        /// <summary>Gets and sets the function to minimize</summary>
        public Func<Vector, double> Function
        {
            get { return _function; }
            set { _function = value; }
        }

        #endregion

        #region Get

        /// <summary>The final status of the solver</summary>
        public SolverStatus Status
        {
            get { return _status; }
        }

        /// <summary> Returns the final value of the function</summary>
        public double Error
        {
            get { return _error; }
        }

        /// <summary>The result of the solver</summary>
        public Vector Result
        {
            get { return _result; }
        }

        /// <summary>Gets the details of the convergence (gradient norm, error)</summary>
        public List<Tuple<double, double>> Convergence
        {
            get { return new List<Tuple<double, double>>(_convergence); }
        }

        /// <summary>Gets the number of times the function was evaluated </summary>
        public int Evaluations
        {
            get { return _evaluations; }
        }

        /// <summary>Gets the maximum number of iterations </summary>
        public int MaxIterations
        {
            get { return _maxIterations; }
        }

        /// <summary>Gets the maxium number of iterations in the line search </summary>
        public int MaxLineSearchIterations
        {
            get { return _maxLineSearchIterations; }
        }

        /// <summary>Gets the optimization type</summary>
        public OptimizationType OptimizationType
        {
            get { return _optimizationType; }
        }

        #endregion

        #endregion

        #region Line searches

        private double LineSearchBranch(double error, Vector x, Vector direction, Vector gradient)
        {
            switch (_lineSearch)
            {
                case LineSearch.Armijo: return ArmijoLineSearch(error, x, direction, gradient);
                case LineSearch.ArmijoGoldStein: return ArmijoGoldsteinLineSearch(error, x, direction, gradient);
                case LineSearch.StrongWolfe: return StrongWolfeLineSearch(error, x, direction, gradient);
                case LineSearch.Lowest: return LowestLineSearch(error, x, direction);
                default: return NaiveLineSearch(error, x, direction);
            }
        }

        private double NaiveLineSearch(double error, Vector x, Vector direction)
        {
            double alpha = 1;
            int k = 0;
            double f = _function(x + (alpha * direction));
            _evaluations++;

            while (double.IsNaN(f) || (Math.Sign(f - error) == -_sign && k < _maxLineSearchIterations))
            {
                alpha *= 0.5;
                f = _function(x + (alpha * direction));
                _evaluations++;
                if (!double.IsNaN(f)) k++;
            }

            return alpha;
        }

        private double LowestLineSearch(double error, Vector x, Vector direction)
        {
            double alpha = 1;

            Dictionary<double, double> valuesDic = new Dictionary<double, double>();
            double f;
            for (int i = 0; i < _maxLineSearchIterations; i++)
            {
                f = _function(x + (alpha * direction));
                _evaluations++;

                if (!double.IsNaN(f) && Math.Sign(f - error) == _sign)
                    valuesDic.Add(alpha, f);
                alpha *= 0.8;
            }

            if (valuesDic.Count == 0) return 0;

            double targetValue = _sign == 1 ? valuesDic.Values.Max() : valuesDic.Values.Min(),
                targetAlpha = valuesDic.Where(kvp => kvp.Value == targetValue).First().Key;

            return targetAlpha;
        }

        private double ArmijoLineSearch(double error, Vector x, Vector direction, Vector gradient)
        {
            double h0 = error,
                c1_d0 = 1e-4 * Vector.Scalar(direction, gradient),
                alpha = 1;
            int k = 0;
            double f = _function(x + (alpha * direction));
            while ((double.IsNaN(f) || Math.Sign(f - error - c1_d0 * alpha) == -_sign) && k < _maxLineSearchIterations)
            {
                _evaluations++;
                alpha *= 0.5;
                k++;
                f = _function(x + (alpha * direction));
            }

            return alpha;
        }

        private double ArmijoGoldsteinLineSearch(double error, Vector x, Vector direction, Vector gradient)
        {
            double h0 = error,
                d0 = Vector.Scalar(direction, gradient),
                alpha = 1,
                c1_d0 = 1e-4 * d0,
                c2_d0 = 0.9 * d0;

            int k = 0;

            while ((Math.Sign(_function(x + (alpha * direction)) - error - c1_d0 * alpha) == -_sign
                ||
                Math.Sign(Vector.Scalar(direction, _gradient(x + (alpha * direction))) - c2_d0) == _sign)
                &&
                k < _maxLineSearchIterations)
            {
                _evaluations++;
                alpha *= 0.5;
                k++;
            }
            return alpha;
        }

        private double StrongWolfeLineSearch(double error, Vector x, Vector direction, Vector gradient)
        {
            double h0 = error,
                d0 = Vector.Scalar(direction, gradient),
                alpha = 1,
                c1_d0 = 1e-4 * d0,
                c2_d0 = 0.9 * d0;
            int k = 0;
            double curvature = Vector.Scalar(direction, _gradient(x + (alpha * direction)));

            while ((Math.Sign(_function(x + (alpha * direction)) - error - c1_d0 * alpha) == -_sign
                ||
                Math.Sign(curvature - c2_d0) == _sign
                ||
                Math.Abs(curvature) > Math.Abs(c2_d0))
                &&
                k < _maxLineSearchIterations)
            {
                _evaluations++;
                alpha *= 0.5;
                curvature = Vector.Scalar(direction, _gradient(x + (alpha * direction)));
                k++;
            }
            return alpha;
        }

        #endregion

        #region Methods

        #region Services
        private Vector NumericalGradient(Vector x, Vector increment)
        {
            double refValue = _function(x);
            _evaluations++;
            Vector result = Vector.Create(x.Size);

            for (int i = 0; i < result.Size; i++)
            {
                Vector xd = x.Clone;
                xd[i] += increment[i];
                result[i] = (_function(xd) - refValue) / increment[i];
                _evaluations++;
            }

            return result;
        }
        #endregion

        /// <summary>Minimizes the function using classic Gradient Descent algorithm</summary>
        /// <param name="momentum">the momentum of the descent</param>
        public void Optimize(double momentum = 0)
        {
            _evaluations = 0;
            if (_function == null) throw new NullReferenceException("function should not be null");

            _descentDirections.Clear();
            _convergence.Clear();

            _result = _initialGuess.Clone;
            _status = SolverStatus.Diverged;
            _error = _function(_result);
            _evaluations++;
            Vector gradient = _gradient(_result),
                direction = _sign * gradient;

            _descentDirections.Add(direction.Clone);
            _convergence.Add(new Tuple<double, double>(gradient.Norm2, _error));
            EndCriteria endCriteria = new EndCriteria(maxIterations: _maxIterations, maxStaticIterations: _maxLineSearchIterations, gradientEpsilon: _gradientThreshold);

            while (!endCriteria.ShouldStop(_error, gradient.Norm2))
            {
                double factor = LineSearchBranch(_error, _result, direction, gradient);
                _result = _result + (factor * direction);

                _error = _function(_result);
                _evaluations++;
                gradient = _gradient(_result);
                direction = (momentum * direction) + (_sign * gradient);

                _descentDirections.Add(direction.Clone);
                _convergence.Add(new Tuple<double, double>(gradient.Norm2, _error));
            }

            _status = endCriteria.Status;
        }

        /// <summary>Minimizes the function using the BFGS gradient descent</summary>
        public void OptimizeBFGS()
        {
            _evaluations = 0;
            if (_function == null) throw new NullReferenceException("function should not be null");

            _convergence.Clear();

            List<Vector> gradients = new List<Vector>();

            _result = _initialGuess.Clone;
            _status = SolverStatus.Diverged;
            _error = _function(_result);
            _evaluations++;
            Vector gradient = _gradient(_result),
                direction = _sign * gradient;
            Matrix B = Matrix.CreateIdentityMatrix(gradient.Size, gradient.Size);

            _descentDirections.Add(direction.Clone);
            _convergence.Add(new Tuple<double, double>(gradient.Norm2, _error));
            gradients.Add(gradient);

            EndCriteria endCriteria = new EndCriteria(maxIterations: _maxIterations, gradientEpsilon: Descents.GRADIENT_EPSILON);
            while (!endCriteria.ShouldStop(_error, gradient.Norm2))
            {
                double factor = LineSearchBranch(_error, _result, direction, gradient);
                Vector s = factor * direction;
                _result = _result + s;

                _error = _function(_result);
                _evaluations++;
                gradient = _gradient(_result);
                gradients.Add(gradient);
                Vector y = gradients[gradients.Count - 1] - gradients[gradients.Count - 2];

                double sBs = Vector.Quadratic(s, B, s);
                if (sBs != 0) B = -((B * s) * (s * B)) / sBs;

                double ys = Vector.Scalar(y, s);
                if (ys != 0) B = B + (y * y) / ys;

                direction = _sign * B.Inverse * gradient;

                _descentDirections.Add(direction.Clone);
                _convergence.Add(new Tuple<double, double>(gradient.Norm2, _error));
            }

            _status = endCriteria.Status;
        }

        #endregion
    }
}
