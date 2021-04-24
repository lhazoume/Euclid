using Euclid.Serialization;
using System;
using System.Xml;

namespace Benchmarking
{
    public class CaseResult : IXmlable
    {
        #region Variables
        private readonly string _name;
        private readonly int _iterations;
        private readonly TimeSpan _timeSpan;
        private readonly long _memoryUsage;
        private readonly DateTime _runDate;
        #endregion

        private CaseResult(string name, int iterations, TimeSpan timeSpan, long memoryUsage, DateTime runDate)
        {
            _name = name;
            _iterations = iterations;
            _timeSpan = timeSpan;
            _runDate = runDate;
            _memoryUsage = memoryUsage;
        }

        #region Accessors
        /// <summary>Gets the case's name</summary>
        public string Name => _name;

        /// <summary>Gets the case's iteration number</summary>
        public int Iterations => _iterations;

        /// <summary>Gets the time taken to run the iterations</summary>
        public TimeSpan TimeSpan => _timeSpan;

        /// <summary>Gets the memory used by the case</summary>
        public long MemoryUsage => _memoryUsage;

        /// <summary>Gets the case run date</summary>
        public DateTime RunDate => _runDate;
        #endregion

        #region Factories
        public static CaseResult Build(string name, int iterations, TimeSpan timeSpan, long memoryUsage)
        {
            return new CaseResult(name, iterations, timeSpan, memoryUsage, DateTime.Now);
        }

        public static CaseResult FromXml(XmlNode xmlNode)
        {
            return null;
        }

        #endregion

        public void ToXml(XmlWriter writer)
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));

            writer.WriteStartElement("result");
            writer.WriteAttributeString("name", _name);
            writer.WriteAttributeString("iterations", _iterations.ToString());
            writer.WriteAttributeString("timeSpan", _timeSpan.Ticks.ToString());
            writer.WriteAttributeString("memoryUsage", _memoryUsage.ToString());
            writer.WriteAttributeString("runDate", _runDate.ToString("ddMMyyyyHHmmss"));
            writer.WriteEndElement();
        }

        public override string ToString()
        {
            return string.Format("{0}({1}) Total={2} Marginal={3} Memory={4}", _name, _iterations, _timeSpan.ToShortString(), new TimeSpan(_timeSpan.Ticks / _iterations).ToShortString(), _memoryUsage.ToString());
        }
    }
}
