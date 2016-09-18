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
    }
}
