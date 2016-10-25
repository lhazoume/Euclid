using System;

namespace Euclid.Helpers
{
    public static class RandomHelper
    {
        /// <summary>
        /// Generates random numbers in an Interval
        /// </summary>
        /// <param name="random">the random number generator</param>
        /// <param name="min">the lower bound of the interval</param>
        /// <param name="max">the upper bound of the interval</param>
        /// <returns>a double</returns>
        public static double NextDouble(this Random random, double min, double max)
        {
            return min + (max - min) * random.NextDouble();
        }
    }
}
