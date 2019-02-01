using System.Xml;

namespace Euclid.Serialization
{
    /// <summary>Interface allowing serialization to an XML files</summary>
    public interface IXmlable
    {
        /// <summary>Serializes a class to Xml </summary>
        /// <param name="writer">the <c>XmlWriter</c></param>
        void ToXml(XmlWriter writer);
    }
}
