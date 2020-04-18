using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Euclid.Analytics
{
    /// <summary>
    /// Interface for predictors
    /// </summary>
    /// <typeparam name="T">the predicted type</typeparam>
    /// <typeparam name="TU">the predictors type</typeparam>
    public interface IPredictor<T, TU>
    {
        /// <summary>Prediction method</summary>
        /// <param name="x">the predictors</param>
        /// <returns>the prediction</returns>
        T Predict(IList<TU> x);
    }
}
