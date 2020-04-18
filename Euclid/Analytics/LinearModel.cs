using System;
using System.Collections.Generic;
using System.Linq;

namespace Euclid.Analytics
{
    /// <summary>
    /// Stores the metrics of a linear regression and allows prediction
    /// </summary>
    public class LinearModel : IPredictor<double, double>
    {
        #region Declarations
        private readonly double _constant, _sse, _ssr, _sst;
        private readonly Vector _factors, _correlations;
        private readonly int _n, _p;
        private readonly bool _succeeded;
        #endregion

        #region Constructors

        /// <summary>Default constructor for a linear model</summary>
        /// <param name="constant">the constant term</param>
        /// <param name="factors">the regression coefficients</param>
        /// <param name="correlations">the zero-degree correlations</param>
        /// <param name="sampleSize">the sample size</param>
        /// <param name="SSE">the sum of squared due to error</param>
        /// <param name="SSR">the sum of squared due to the regression</param>
        /// <param name="succeeded">the status of the regression</param>
        private LinearModel(double constant, double[] factors, double[] correlations, int sampleSize, double SSE, double SSR, bool succeeded)
        {
            if (factors == null) throw new ArgumentNullException(nameof(factors));
            if (correlations == null) throw new ArgumentNullException(nameof(correlations));

            _succeeded = succeeded;

            _p = factors.Length;
            _n = sampleSize;

            _sse = SSE;
            _ssr = SSR;
            _sst = _ssr + _sse;

            _constant = constant;
            _factors = Vector.Create(_p);
            _correlations = Vector.Create(_p);
            for (int i = 0; i < _p; i++)
            {
                _factors[i] = factors[i];
                _correlations[i] = correlations[i];
            }
        }

        /// <summary>Builds a linear model for a failed regression</summary>
        public LinearModel()
            : this(0, new double[] { 0 }, new double[] { 0 }, 0, 0, 0, false)
        { }

        /// <summary> Builds a constant linear model</summary>
        /// <param name="constant">the constant</param>
        /// <param name="sampleSize">the sample size</param>
        /// <param name="SSE">the sum of squares due to error</param>
        public LinearModel(double constant, int sampleSize, double SSE)
            : this(constant, new double[] { 0 }, new double[] { 0 }, sampleSize, SSE, 0, true)
        { }

        /// <summary> Builds a linear model for a succesful regression </summary>
        /// <param name="constant">the regression constant term</param>
        /// <param name="factors">the regression linear coefficients</param>
        /// <param name="correlations">the zero-degree correlations</param>
        /// <param name="sampleSize">the sample size</param>
        /// <param name="SSE">the sum of squares due to the error</param>
        /// <param name="SSR">the sum of squares due to the regression</param>
        public LinearModel(double constant, double[] factors, double[] correlations, int sampleSize, double SSE, double SSR)
            : this(constant, factors, correlations, sampleSize, SSE, SSR, true)
        { }

        #endregion

        #region Accessors
        /// <summary>Gets the constant term</summary>
        public double Constant
        {
            get { return _succeeded ? _constant : 0; }
        }

        /// <summary>Gets the linear terms</summary>
        public Vector Factors
        {
            get { return _succeeded ? _factors : Vector.Create(0.0); }
        }

        /// <summary>Gets the correlations between the explanatory variables and the regressand </summary>
        public Vector Correlations
        {
            get { return _succeeded ? _correlations : Vector.Create(0.0); }
        }

        /// <summary>Gets the sample size</summary>
        public int SampleSize
        {
            get { return _n; }
        }

        /// <summary>Gets the R² on the sample data</summary>
        public double R2
        {
            get { return _succeeded ? _ssr / _sst : 0; }
        }

        /// <summary>
        /// Gets the adjusted R² on the sample data
        /// </summary>
        public double AdjustedR2
        {
            get
            {
                if (_succeeded)
                    return _sst == 0 ? 0 : 1 - _sse * (_n - 1) / (_sst * (_n - _factors.Data.Count(f => f != 0) - 1));
                return 0;
            }
        }

        /// <summary>
        /// specifies whether the regression succeeds
        /// </summary>
        public bool Succeeded
        {
            get { return _succeeded; }
        }

        /// <summary>
        /// Gets the sum of squares due to error
        /// </summary>
        public double SSE
        {
            get { return _sse; }
        }

        /// <summary>
        /// Gets the sum of squares due to the regression
        /// </summary>
        public double SSR
        {
            get { return _ssr; }
        }

        /// <summary>
        /// Gets the total sum of squares
        /// </summary>
        public double SST
        {
            get { return _sst; }
        }
        #endregion

        #region ToString
        /// <summary>Returns a string that represents the linear model</summary>
        /// <returns>a string that represents the linear model</returns>
        public override string ToString()
        {
            return string.Format("[R2={0}];[AdjustedR2={1}];[Constant={2}]", R2, AdjustedR2, _constant);
        }
        #endregion

        #region IPredictor
        /// <summary>
        /// Returns the estimator for the given set of data
        /// </summary>
        /// <param name="x">the set of regressors</param>
        /// <returns>the estimator of the regressed data</returns>
        public double Predict(IList<double> x)
        {
            if (x == null) throw new ArgumentNullException(nameof(x));

            double y = _constant;
            if (_succeeded)
                for (int i = 0; i < Math.Min(_factors.Size, x.Count); i++)
                    y += _factors[i] * x[i];
            return y;
        }

        /// <summary>
        /// Returns the estimator for the given set of data
        /// </summary>
        /// <param name="x">the set of regressors</param>
        /// <returns>the estimator of the regressed data</returns>
        public double Predict(Vector x)
        {
            return _constant + (_succeeded ? Vector.Scalar(x, _factors) : 0);
        }
        #endregion
    }
}
