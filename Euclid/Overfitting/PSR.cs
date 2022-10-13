using Euclid.Distributions.Continuous;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Euclid.Overfitting
{
    /// <summary>
    /// Class which computes Probilitistic Sharpe ratio & minimal Track Record Length (minTRL) , works came from Baily, Lopez de Prado 2012/2014
    /// </summary>
    public static class PSR
    {
        #region methods
        /// <summary>
        /// Compute the probabilistic sharpe ratio according the observable sharpe & its higher moments distribution
        /// </summary>
        /// <param name="oSR">Sharpe Ratio from the observable returns</param>
        /// <param name="kappa">The skewness of the observed returns</param>
        /// <param name="lambda">The kurtosis of the observed returns</param>
        /// <param name="T">The number of observed returns</param>
        /// <param name="tSR">True sharpe ratio tested or reference sharpe ratio</param>
        /// <returns>PSR value</returns>
        /// <exception cref="Exception">The number of observed returns</exception>
        public static double computePSR(double oSR, double kappa, double lambda, int T, double tSR)
        {
            if (T <= 0) throw new Exception($"T must superior to 0 (T={T})");

            NormalDistribution norm = new NormalDistribution();
            double SE = Math.Sqrt((1 - (kappa * oSR) + ((lambda - 1) * (Math.Pow(oSR, 2) / 4))) / T); // standard error of observable sharpe ratio oSR

            return norm.CumulativeDistribution((oSR - tSR)/SE);
        }

        /// <summary>
        /// Compute the minimal Track Record Length (minTRL) according the observable sharpe & its higher moments distribution
        /// </summary>
        /// <param name="oSR">Sharpe Ratio from the observable returns</param>
        /// <param name="kappa">The skewness of the observed returns</param>
        /// <param name="lambda">The kurtosis of the observed returns</param>
        /// <param name="T">The number of observed returns</param>
        /// <param name="tSR">True sharpe ratio tested or reference sharpe ratio</param>
        /// <param name="alpha">Threshold of confidence interval</param>
        /// <returns>minTRL value</returns>
        public static double computeMinTRL(double oSR, double kappa, double lambda, int T, double tSR, double alpha)
        {
            if (T <= 0) throw new Exception($"T must superior to 0 (T={T})");

            NormalDistribution norm = new NormalDistribution();
            double zAlpha = norm.InverseCumulativeDistribution(alpha);

            return 1 + (1 - (kappa * oSR) + ((lambda - 1) * (Math.Pow(oSR, 2) / 4))) * Math.Pow(zAlpha / (oSR - tSR), 2);
        }
        #endregion
    }
}
