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
    /// <typeparam name="U">the predictors type</typeparam>
    public interface IPredictor<T, U>
    {
        /// <summary>Prediction method</summary>
        /// <param name="x">the predictors</param>
        /// <returns>the prediction</returns>
        T Predict(IList<U> x);
    }
}
