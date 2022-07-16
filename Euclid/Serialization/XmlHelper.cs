using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace Euclid.Serialization
{
    /// <summary>Helper class for IXmlable classes</summary>
    public static class XmlHelper
    {
        /// <summary>Saves the class' XML representation</summary>
        /// <param name="xmlable">the IXmlable class</param>
        /// <param name="filePath">the target file path</param>
        public static void SaveXml(this IXmlable xmlable, string filePath)
        {
            File.WriteAllText(filePath, xmlable.GetXml());
        }

        /// <summary>Writes the class'XML representation to a string</summary>
        /// <param name="xmlable">the IXmlable class</param>
        /// <returns>a string</returns>
        public static string GetXml(this IXmlable xmlable)
        {
            if (xmlable == null) throw new ArgumentNullException(nameof(xmlable));

            StringBuilder builder = new StringBuilder();
            StringWriterWithEncoding stringWriter = new StringWriterWithEncoding(builder, Encoding.UTF8);
            XmlWriterSettings settings = new XmlWriterSettings() { Indent = true, CloseOutput = true };
            using (XmlWriter writer = XmlWriter.Create(stringWriter, settings))
            {
                writer.WriteStartDocument();
                xmlable.ToXml(writer);
                writer.WriteEndDocument();
                writer.Flush();
            }

            return builder.ToString();
        }

        /// <summary>Reads a file to an XMl node </summary>
        /// <param name="filePath">the target file</param>
        /// <returns>an <c>XmlNode</c></returns>
        public static XmlNode ReadXml(string filePath)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);

            return doc;
        }

        /// <summary>Serializes a date to Euclid's specific date format </summary>
        /// <param name="date">the date</param>
        /// <returns>a string</returns>
        public static string ToEuclidDateString(this DateTime date)
        {
            return $"{date.Year}{date.Month:00}{date.Day:00}";
        }

        /// <summary>De-serializes a string to a date </summary>
        /// <param name="text">the text to convert to a date</param>
        /// <returns>a DateTime</returns>
        public static DateTime FromEuclidDateString(this string text)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            return new DateTime(int.Parse(text.Substring(0, 4)), int.Parse(text.Substring(4, 2)), int.Parse(text.Substring(6, 2)));
        }
    }
    /// <summary>Helps building a list of objects from their serialized representation</summary>
    /// <typeparam name="T">the object type</typeparam>
    public static class BuildList<T> where T : IXmlable
    {
        /// <summary>Builds a list of objects from an XML node list</summary>
        /// <param name="nodeList">the node list</param>
        /// <param name="builder">the function that turns an XML node into an instance of an object </param>
        /// <returns>a list of objects</returns>
        public static List<T> FromXmls(XmlNodeList nodeList, Func<XmlNode, T> builder)
        {
            if (nodeList == null) throw new ArgumentNullException(nameof(nodeList));

            List<T> result = new List<T>();
            foreach (XmlNode node in nodeList)
                result.Add(builder(node));
            return result;
        }
    }
}
