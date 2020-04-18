using System;
using System.Collections.Generic;
using System.Linq;

namespace Euclid.Extensions
{
    /// <summary>Helper class for random number generation</summary>
    public static class Randomizer
    {
        /// <summary>Generates random numbers in an Interval</summary>
        /// <param name="random">the random number generator</param>
        /// <param name="min">the lower bound of the interval</param>
        /// <param name="max">the upper bound of the interval</param>
        /// <returns>a double</returns>
        public static double NextDouble(this Random random, double min, double max)
        {
            return min + (max - min) * random.NextDouble();
        }

        /// <summary>Shuffles a list of template type</summary>
        /// <typeparam name="T">the template type</typeparam>
        /// <param name="list">the list to shuffle</param>
        /// <param name="random">the random number generator</param>
        public static void Shuffle<T>(this IList<T> list, Random random)
        {
            if (random == null) return;
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        /// <summary>Creates a shuffled version of a list</summary>
        /// <typeparam name="T">the template type</typeparam>
        /// <param name="list">the list to shuffle</param>
        /// <param name="random">the random number generator</param>
        /// <returns>a shuffled version of the list</returns>
        public static List<T> CloneAndShuffle<T>(this IList<T> list, Random random)
        {
            List<int> indexes = Enumerable.Range(0, list.Count).ToList();
            indexes.Shuffle(random);
            return indexes.Select(i => list[i]).ToList();
        }

        /// <summary>Picks random number</summary>
        /// <param name="fromIncluded"></param>
        /// <param name="toExcluded"></param>
        /// <param name="numberOfPicks"></param>
        /// <param name="randomNumberGenerator"></param>
        /// <returns></returns>
        public static List<int> PickRandomNumbers(int fromIncluded, int toExcluded, int numberOfPicks, Random randomNumberGenerator)
        {
            if (randomNumberGenerator == null) throw new ArgumentNullException(nameof(randomNumberGenerator));
            List<int> result = new List<int>();

            while (result.Count < numberOfPicks)
            {
                int randomNumber = randomNumberGenerator.Next(fromIncluded, toExcluded);
                if (!result.Contains(randomNumber))
                    result.Add(randomNumber);
            }

            return result;
        }

        /// <summary>Picks random number</summary>
        /// <param name="fromIncluded">the lower bound (included)</param>
        /// <param name="toExcluded">the upper bound (excluded)</param>
        /// <param name="numberOfPicks">the number of number wanted</param>
        /// <param name="forbiddenValues">the forbidden values</param>
        /// <param name="randomNumberGenerator">a random object</param>
        /// <returns>a list of numbers</returns>
        public static List<int> PickRandomNumbers(int fromIncluded, int toExcluded, int numberOfPicks, IEnumerable<int> forbiddenValues, Random randomNumberGenerator)
        {
            if (randomNumberGenerator == null) throw new ArgumentNullException(nameof(randomNumberGenerator));

            List<int> result = new List<int>();

            while (result.Count < numberOfPicks)
            {
                int randomNumber = randomNumberGenerator.Next(fromIncluded, toExcluded);
                if (!result.Contains(randomNumber) && !forbiddenValues.Contains(randomNumber))
                    result.Add(randomNumber);
            }

            return result;
        }
    }
}
