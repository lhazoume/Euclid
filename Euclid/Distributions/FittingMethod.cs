namespace Euclid.Distributions
{
    /// <summary>Defines the method used to fit data to a distribution</summary>
    public enum FittingMethod
    {
        /// <summary>Fits the parameters to the moments</summary>
        Moments = 0,
        /// <summary>Fits the parameters by using the maximum likehood</summary>
        MaximumLikelihood = 1,
        /// <summary>Fits the parameters by using the positional arguments</summary>
        PositionalArgument = 2,
        /// <summary>Fits the parameters by using the least square method</summary>
        LeastSquare = 3
    }
}
