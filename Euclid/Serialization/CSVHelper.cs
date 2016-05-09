using System.Globalization;

namespace Euclid.Serialization
{
    public static class CSVHelper
    {
        public static string Separator = CultureInfo.CurrentCulture.TextInfo.ListSeparator;
    }
}
