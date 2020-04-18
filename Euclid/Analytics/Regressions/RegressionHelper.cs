using Euclid.DataStructures.IndexedSeries;
using System;
using System.Text;

namespace Euclid.Analytics.Regressions
{
    /// <summary>A helper for the regression</summary>
    public static class RegressionHelper
    {
        /// <summary>Displays the regression result as a string</summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TV"></typeparam>
        /// <param name="dataFrame">the dataframe</param>
        /// <param name="series">the series</param>
        /// <param name="linearModel">the linear model</param>
        /// <param name="format">the format for the coefficients</param>
        /// <returns>a string</returns>
        public static string Text<T, TV>(DataFrame<T, double, TV> dataFrame, Series<T, double, TV> series, LinearModel linearModel, string format)
            where T : IComparable<T>, IEquatable<T>
            where TV : IEquatable<TV>, IConvertible
        {
            if (dataFrame == null) throw new ArgumentNullException(nameof(dataFrame));
            if (series == null) throw new ArgumentNullException(nameof(series));
            if (linearModel == null) throw new ArgumentNullException(nameof(linearModel));

            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format("{0}={1}", series.Label.ToString(), string.Format(format, linearModel.Constant)));
            for (int i = 0; i < linearModel.Factors.Size; i++)
                sb.AppendFormat(string.Format("{0}*{1}", string.Format(format, linearModel.Factors[i]), dataFrame.GetLabel(i).ToString()));
            sb.Append(Environment.NewLine);
            sb.Append(string.Format("R²={0:0.#}% Adj={1:0.#}%", 100 * linearModel.R2, 100 * linearModel.AdjustedR2));
            return sb.ToString();
        }
    }
}
