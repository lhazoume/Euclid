using System.Xml;

namespace Euclid.Serialization
{
    /// <summary>
    /// Interface allowing serialization and de-serialization from/to XML files
    /// </summary>
    public interface IXmlable
    {
        /// <summary>Serializes a class to Xml </summary>
        /// <param name="writer">the <c>XmlWriter</c></param>
        void ToXml(XmlWriter writer);

        /// <summary>De-serializes an Xml node to fill the class</summary>
        /// <param name="node">the <c>XmlNode</c></param>
        void FromXml(XmlNode node);
    }
}
