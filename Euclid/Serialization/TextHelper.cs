using System;
using System.Text;

namespace Euclid.Serialization
{
    /// <summary>uncomplete helper class</summary>
    public static class TextHelper
    {
        /// <summary>Serializes a TimeSpan</summary>
        /// <param name="timeSpan">the time span</param>
        /// <returns>a string</returns>
        public static string ToShortString(this TimeSpan timeSpan)
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (timeSpan.Days != 0) stringBuilder.Append(string.Format("{0:#0}d", timeSpan.Days));
            if (timeSpan.Hours != 0) stringBuilder.Append(string.Format("{0:#0}h", timeSpan.Hours));
            if (timeSpan.Minutes != 0) stringBuilder.Append(string.Format("{0:00}m", timeSpan.Minutes));
            if (timeSpan.Seconds != 0) stringBuilder.Append(string.Format("{0:00}s", timeSpan.Seconds));
            if (timeSpan.Milliseconds != 0) stringBuilder.Append(string.Format("{0:000}ms", timeSpan.Milliseconds));
            return stringBuilder.ToString();
        }
    }
}
