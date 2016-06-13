namespace Euclid.Analytics.Regressions
{
    /// <summary>
    /// Returns the status of the regression
    /// </summary>
    public enum RegressionStatus
    {
        /// <summary> the regression did not run </summary>
        NotRan = 0,
        /// <summary> the regression ran as expected </summary>
        Normal = 1,
        /// <summary> the regression exceeded its max iterations </summary>
        IterationExceeded = 2,
        /// <summary> the regression failed </summary>
        BadData = 3
    }
}
