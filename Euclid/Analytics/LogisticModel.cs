using System;
using System.Collections.Generic;
using System.Linq;

namespace Euclid.Analytics
{
    /// <summary>
    /// Stores the metrics of a logistic regression and allows prediction
    /// </summary>
    public class LogisticModel : IPredictor<double, double>
    {
        #region Declarations
        private readonly double _constant, _r2;
        private readonly Vector _factors;
        private readonly int _p;
        private readonly bool _succeeded;
        #endregion

        #region Constructors

        /// <summary>Default constructor for a linear model</summary>
        /// <param name="constant">the constant term</param>
        /// <param name="factors">the regression coefficients</param>
        /// <param name="r2">the R squared</param>
        /// <param name="succeeded">the status of the regression</param>
        private LogisticModel(double constant, double[] factors, double r2, bool succeeded)
        {
            _succeeded = succeeded;
            _p = factors.Length;
            _constant = constant;
            _r2 = r2;
            _factors = Vector.Create(factors);
        }

        /// <summary> Builds a constant linear model</summary>
        /// <param name="constant">the constant</param>
        /// <param name="r2">the R squared</param>
        public LogisticModel(double constant, double r2)
            : this(constant, new double[] { 0 }, r2, false)
        { }

        /// <summary> Builds a linear model for a succesful regression </summary>
        /// <param name="constant">the regression constant term</param>
        /// <param name="factors">the regression linear coefficients</param>
        /// <param name="r2">the R squared</param>
        public LogisticModel(double constant, double[] factors, double r2)
            : this(constant, factors, r2, true)
        { }

        #endregion

        #region Accessors
        /// <summary>Gets the constant term</summary>
        public double Constant
        {
            get { return _succeeded ? _constant : 0; }
        }

        /// <summary>Returns the R-squared</summary>
        public double R2
        {
            get { return _r2; }
        }
        /// <summary>Gets the linear terms</summary>
        public Vector Factors
        {
            get { return _succeeded ? _factors : Vector.Create(0.0); }
        }

        /// <summary>specifies whether the regression succeeds</summary>
        public bool Succeeded
        {
            get { return _succeeded; }
        }
        #endregion

        #region ToString
        /// <summary>Returns a string that represents the linear model</summary>
        /// <returns>a string that represents the linear model</returns>
        public override string ToString()
        {
            return string.Format("[Constant={0}]", _constant);
        }
        #endregion

        #region IPredictor
        /// <summary>Returns the estimator for the given set of data</summary>
        /// <param name="x">the set of regressors</param>
        /// <returns>the estimator of the regressed data</returns>
        public double Predict(IList<double> x)
        {
            double y = _constant;
            if (_succeeded)
                for (int i = 0; i < Math.Min(_factors.Size, x.Count); i++)
                    y += _factors[i] * x[i];
            return Fn.LogisticFunction(y);
        }

        /// <summary>Returns the estimator for the given set of data</summary>
        /// <param name="x">the set of regressors</param>
        /// <returns>the estimator of the regressed data</returns>
        public double Predict(Vector x)
        {
            return Fn.LogisticFunction(_constant + (_succeeded ? Vector.Scalar(x, _factors) : 0));
        }
        #endregion
    }
}
