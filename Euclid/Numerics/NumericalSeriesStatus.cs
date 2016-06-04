namespace Euclid.Numerics
{
    /// <summary>The status of the numerical series status</summary>
    public enum NumericalSeriesStatus
    {
        /// <summary>The cumulator did not run</summary>
        NotRan = 0,
        /// <summary>The cumulator diverged</summary>
        Diverged = 1,
        /// <summary>The cumulator ran as expected</summary>
        Normal = 2,
        /// <summary>The cumulator exceeded its maximum number of iterations</summary>
        IterationExceeded = 3
    }
}
