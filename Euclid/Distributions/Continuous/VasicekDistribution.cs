using Euclid.Histograms;
using System;

namespace Euclid.Distributions.Continuous
{
    /// <summary>Vasicek distribution class</summary>
    public class VasicekDistribution : ContinuousDistribution
    {
        #region Declarations
        private readonly double _p, _rho;
        #endregion

        #region Constructors
        /// <summary>Builds a Vasicek distribution</summary>
        /// <param name="p">the average defaut rate</param>
        /// <param name="rho">the asset correlation</param>
        public VasicekDistribution(double p, double rho)
        {
            if (p <= 0 || p >= 1) throw new ArgumentException("The parameter p must be between 0 and 1");
            if (rho <= 0 || rho >= 1) throw new ArgumentException("The parameter rho must be between 0 and 1");
            _p = p;
            _rho = rho;
            _support = new Interval(0, 1, true, true);
        }
        #endregion

        #region Accessors
        /// <inheritdoc/>
        public override double Mean => _p;

        /// <inheritdoc/>
        public override double Median => Fn.Phi(Fn.InvPhi(_p) / Math.Sqrt(1 - _rho));

        /// <inheritdoc/>
        public override double Mode
        {
            get
            {
                if (_rho < 0.5)
                    return Fn.Phi(Fn.InvPhi(_p) * Math.Sqrt(1 - _rho) / (1 - 2 * _rho));
                else if (_rho == 0.5)
                {
                    if(_p < 0.5) return 0;
                    else if (_p > 0.5) return 1;
                    else return 0.5;
                }
                else
                    return 0.0;
            }
        }

        /// <inheritdoc/>
        public override double StandardDeviation => Math.Sqrt(Variance);

        /// <inheritdoc/>
        public override double Variance => Fn.Phi2(Fn.InvPhi(_p), Fn.InvPhi(_p), _rho) - Math.Pow(_p, 2);

        /// <inheritdoc/>
        public override double Skewness => throw new NotImplementedException();

        /// <inheritdoc/>
        public override double Entropy => throw new NotImplementedException();

        /// <inheritdoc/>
        public override Interval Support => _support;
        #endregion

        #region Methods
        /// <inheritdoc/>
        public override double CumulativeDistribution(double x)
        {
            return Fn.Phi((Math.Sqrt(1 - _rho) * Fn.InvPhi(x) - Fn.InvPhi(_p)) / Math.Sqrt(_rho));
        }

        /// <inheritdoc/>
        public override double InverseCumulativeDistribution(double p)
        {
            return Fn.Phi((Fn.InvPhi(_p) + Math.Sqrt(_rho) * Fn.InvPhi(p)) / Math.Sqrt(1 - _rho));
        }

        /// <inheritdoc/>
        public override double MomentGeneratingFunction(double t)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override double ProbabilityDensity(double x)
        {
            return Math.Sqrt((1 - _rho) / _rho) * Math.Exp(0.5 * (Math.Pow(Fn.InvPhi(x), 2) - Math.Pow((Math.Sqrt(1 - _rho) * Fn.InvPhi(x) - Fn.InvPhi(_p)) / Math.Sqrt(_rho), 2)));
        }
        #endregion
    }
}
