using System.Collections.Generic;

namespace Euclid.IndexedSeries.Analytics
{
    /// <summary>
    /// Interface for predictors
    /// </summary>
    /// <typeparam name="T">the predicted type</typeparam>
    /// <typeparam name="U">the predictors type</typeparam>
    public interface IPredictor<T, U>
    {
        /// <summary>Prediction method</summary>
        /// <param name="x">the predictors</param>
        /// <returns>the prediction</returns>
        T Predict(IList<U> x);
    }
}
