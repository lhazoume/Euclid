using System.Collections.Generic;

namespace Euclid.IndexedSeries.Analytics
{
    public interface IPredictor<T, U>
    {
        T Predict(IList<U> x);
    }
}
