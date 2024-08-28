using Euclid.Distributions.Continuous;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace Euclid.Analytics.Statistics.Tests
{
    /// <summary>Helps to do student test</summary>
    internal static class StudentTest
    {
        /// <summary>
        /// Test the specified mean to the empirical mean of the sample
        /// </summary>
        /// <param name="specifiedMean">The specified mean of the sample</param>
        /// <param name="sample">The sample to test</param>
        /// <param name="rejectionRegion">The test laterality</param>
        /// <param name="signifianceLevel">The signifiance level of the test</param>
        /// <returns>The acceptance of the specified mean</returns>
        internal static bool MonoSampleMeanTest(double specifiedMean, double[] sample, double signifianceLevel, RejectionRegion rejectionRegion)
        {
            int sampleSize = sample.Length;
            double empiricMean = sample.Average(),
                estimatedVariance = (Vector.Create(sample) - empiricMean).SumOfSquares / (sampleSize - 1),
                estimatedStandardDeviation = Math.Sqrt(estimatedVariance),
                tStatistics = Math.Sqrt(sampleSize) * (empiricMean - specifiedMean) / estimatedStandardDeviation;

            StudentDistribution studentDistribution = new StudentDistribution(sampleSize - 1);

            if (rejectionRegion == RejectionRegion.Bilateral)
                return Math.Abs(tStatistics) < studentDistribution.InverseCumulativeDistribution(1 - signifianceLevel / 2);
            else if (rejectionRegion == RejectionRegion.Left)
                return tStatistics > studentDistribution.InverseCumulativeDistribution(signifianceLevel);
            else
                return tStatistics < studentDistribution.InverseCumulativeDistribution(1 - signifianceLevel);
        }
    }
}
