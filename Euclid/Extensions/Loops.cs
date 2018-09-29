using System;
using System.Threading.Tasks;

namespace Euclid.Extensions
{
    /// <summary>Handles the loops</summary>
    public static class Loops
    {
        /// <summary>Executes a loop which can be made sequential or parallel on demand</summary>
        /// <param name="fromInclusive">the start index, included</param>
        /// <param name="toExclusive">the end index, excluded</param>
        /// <param name="body">the action executed on every loop</param>
        /// <param name="isParallel">determines whether the loop is sequential or parallel</param>
        public static void For(int fromInclusive, int toExclusive, bool isParallel, Action<int> body)
        {
            if (isParallel)
                Parallel.For(fromInclusive, toExclusive, body);
            else
                for (int i = fromInclusive; i < toExclusive; i++)
                    body(i);
        }
    }
}
