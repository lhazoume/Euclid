namespace Euclid.Distributions
{
    /// <summary> Parametric distribution interface, expose Fit method</summary>
    interface IParametricDistribution
    {
        /// <summary>Fits the distribution to a sample of data</summary>
        /// <param name="sample">the sample of data to fit</param>
        /// <param name="fitting">the fitting method</param>
        void Fit(FittingMethod fitting, double[] sample);
    }
}
