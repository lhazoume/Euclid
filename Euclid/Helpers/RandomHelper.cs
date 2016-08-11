using System;

namespace Euclid.Helpers
{
    public static class RandomHelper
    {
        public static double NextDouble(this Random random, double min, double max)
        {
            return min + (max - min) * random.NextDouble();
        }
    }
}
