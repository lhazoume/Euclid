using System.IO;
using System.Text;

namespace Euclid.Serialization
{
    /// <summary>Overrides the string writer to allow for a specific encoding</summary>
    public class StringWriterWithEncoding : StringWriter
    {
        private readonly Encoding _encoding;
        public StringWriterWithEncoding(StringBuilder sb, Encoding encoding) : base(sb)
        { this._encoding = encoding; }

        public override Encoding Encoding => _encoding;
    }
}
