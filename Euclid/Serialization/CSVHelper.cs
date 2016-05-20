using System.Globalization;

namespace Euclid.Serialization
{
    /// <summary>
    /// CSV Helper
    /// </summary>
    public static class CSVHelper
    {
        /// <summary>
        /// The culture's list separator
        /// </summary>
        public static string Separator = CultureInfo.CurrentCulture.TextInfo.ListSeparator;
    }
}
