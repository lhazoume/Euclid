using System.IO;
using System.Text;
using System.Xml;

namespace Euclid.Serialization
{
    /// <summary>
    /// Helper class for IXmlable classes
    /// </summary>
    public static class XmlHelper
    {
        /// <summary>
        /// Saves the class' XML representation
        /// </summary>
        /// <param name="xmlable">the IXmlable class</param>
        /// <param name="filePath">the target file path</param>
        public static void SaveXml(this IXmlable xmlable, string filePath)
        {
            File.WriteAllText(filePath, xmlable.GetXml());
        }

        public static string GetXml(this IXmlable xmlable)
        {

            StringBuilder builder = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings() { Indent = true };
            XmlWriter writer = XmlWriter.Create(builder, settings);

            writer.WriteStartDocument();
            xmlable.ToXml(writer);
            writer.WriteEndDocument();
            writer.Flush();

            return builder.ToString();
        }
    }
}
