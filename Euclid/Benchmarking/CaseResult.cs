using Euclid.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Euclid.Serialization;
using System;
using System.Diagnostics.Contracts;
using System.Management;
using System.Text;
using System.Xml;

namespace Euclid.Benchmarking
{
    public class CaseResult : IXmlable
    {
        #region Variables
        private readonly string _name;
        private readonly int _iterations;
        private readonly TimeSpan _timeSpan;
        private readonly long _memoryUsage;
        private readonly DateTime _runDate;
        private readonly static ManagementObjectSearcher _baseboardSearcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_BaseBoard"),
            _processorSearcher = new ManagementObjectSearcher("select * from Win32_Processor"),
            _gpuSearcher = new ManagementObjectSearcher("select * from Win32_VideoController");
        private readonly string _serialNumber, _processor, _gpu;
        #endregion

        #region Constructors
        private CaseResult(string name, int iterations, TimeSpan timeSpan, long memoryUsage, DateTime runDate, string serialNumber, string processor, string gpu)
        {
            _name = name;
            _iterations = iterations;
            _timeSpan = timeSpan;
            _runDate = runDate;
            _memoryUsage = memoryUsage;
            _serialNumber = serialNumber;
            _processor = processor;
            _gpu = gpu;
        }

        private CaseResult(string name, int iterations, TimeSpan timeSpan, long memoryUsage, DateTime runDate)
        {
            _name = name;
            _iterations = iterations;
            _timeSpan = timeSpan;
            _runDate = runDate;
            _memoryUsage = memoryUsage;
            _serialNumber = SerialNumber;
            _processor = Processor;
            _gpu = GPU;
        }
        #endregion

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

        /// <summary>Get the motherboard's serial number</summary>
        public static string SerialNumber
        {
            get
            {
                try
                {
                    foreach (ManagementObject queryObj in _baseboardSearcher.Get())
                        return queryObj["SerialNumber"].ToString();
                    return "";
                }
                catch (Exception) { return ""; }
            }
        }

        public static string Processor
        {
            get
            {
                try
                {
                    foreach (ManagementObject obj in _processorSearcher.Get())
                        return $"Name={obj["Name"]} - Sped={obj["CurrentClockSpeed"]} - Cores={obj["NumberOfCores"]} - Processors={obj["NumberOfLogicalProcessors"]} - Type={obj["ProcessorType"]}";
                    return "";
                }
                catch (Exception) { return ""; }
            }
        }

        public static string GPU
        {
            get
            {
                try
                {
                    foreach (ManagementObject obj in _gpuSearcher.Get())
                        return $"Name={obj["Name"]} - RAM={SizeSuffix((long)Convert.ToDouble(obj["AdapterRAM"]))}";
                    return "";
                }
                catch (Exception) { return ""; }
            }
        }
        #endregion

        #region Factories
        public static CaseResult Build(string name, int iterations, TimeSpan timeSpan, long memoryUsage)
        {
            return new CaseResult(name, iterations, timeSpan, memoryUsage, DateTime.Now);
        }

        public CaseResult FromXml(XmlNode node)
        {
            if (node == null)
                return null;
            string name = node.Attributes["name"].Value,
                motherBoard = node.Attributes["motherboard"].Value,
                processor = node.Attributes["processor"].Value,
                gpi = node.Attributes["gpu"].Value;
            throw new NotImplementedException();

            //private readonly int _iterations;
            //private readonly TimeSpan _timeSpan;
            //private readonly long _memoryUsage;
            //private readonly DateTime _runDate;

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
            writer.WriteAttributeString("motherboard", _serialNumber);
            writer.WriteAttributeString("processor", _processor);
            writer.WriteAttributeString("gpu", _gpu);
            writer.WriteEndElement();
        }

        public override string ToString()
        {
            return string.Format("{0}({1}) Total={2} Marginal={3} Memory={4}", _name, _iterations, _timeSpan.ToShortString(), new TimeSpan(_timeSpan.Ticks / _iterations).ToShortString(), _memoryUsage.ToString());
        }

        private static readonly string[] SizeSuffixes = { "o", "Ko", "Mo", "Go", "To" };

        private static string SizeSuffix(Int64 value)
        {
            if (value < 0) { return "-" + SizeSuffix(-value); }
            if (value == 0) { return "0.0 bytes"; }

            int mag = (int)Math.Log(value, 1024);
            decimal adjustedSize = (decimal)value / (1L << (mag * 10));

            return string.Format("{0:n1} {1}", adjustedSize, SizeSuffixes[mag]);
        }
    }
}
