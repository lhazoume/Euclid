namespace Euclid.Serialization
{
    /// <summary>
    /// Interface allowing serialization and de-serialization from/to CSV files
    /// </summary>
    public interface ICSVable
    {
        /// <summary>Builds a string representation of the content of the class </summary>
        /// <returns>a <c>String</c></returns>
        string ToCSV();

        /// <summary>Fills a class from a string</summary>
        /// <param name="text">the <c>String</c> content</param>
        void FromCSV(string text);
    }
}
