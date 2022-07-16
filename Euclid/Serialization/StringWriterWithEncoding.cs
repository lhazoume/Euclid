using System.IO;
using System.Text;

namespace Euclid.Serialization
{
    /// <summary>Overrides the string writer to allow for a specific encoding</summary>
    public class StringWriterWithEncoding : StringWriter
    {
        #region Variables
        private readonly Encoding _encoding;
        #endregion

        #region Constructors
        /// <summary>Builds a StringWrtiter with the possibility to specify the encoding</summary>
        /// <param name="sb">the underlying StringBuilder</param>
        /// <param name="encoding">the Encoding</param>
        public StringWriterWithEncoding(StringBuilder sb, Encoding encoding)
            : base(sb)
        { 
            this._encoding = encoding; 
        }
        #endregion

        #region Accessors
        /// <summary>Returns the StringWriter's encoding</summary>
        public override Encoding Encoding => _encoding;
        #endregion
    }
}
