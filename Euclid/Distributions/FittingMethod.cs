namespace Euclid.Distributions
{
    /// <summary>Defines the method used to fit data to a distribution</summary>
    public enum FittingMethod
    {
        /// <summary>Fits the parameters to the moments</summary>
        Moments = 0,
        /// <summary>Fits the parameters by using the maximum likehood</summary>
        MaximumLikelihood = 1
    }
}
