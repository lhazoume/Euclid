using System.Xml;

namespace Euclid.Serialization
{
    public interface IXmlable
    {
        void ToXml(XmlWriter writer);
        void FromXml(XmlNode node);
    }
}
