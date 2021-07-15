using Euclid.DataStructures;
using System;
using System.IO;
using System.Linq;

namespace Euclid.Logging
{
    /// <summary>Represents a logger that dumps itself into a file</summary>
    public class FileLogger : ILogger
    {
        private readonly SelfFlushedQueue<LogRecord> _queue;
        private readonly Level _minLevel, _maxLevel;

        /// <summary>Builds a file logger</summary>
        /// <param name="maxRecords">the maximum number of logs before dumping to a file</param>
        /// <param name="pathFormat">the general format of the file path</param>
        /// <param name="minLevel">the minimum level needed to be logged</param>
        /// <param name="maxLevel">the maximum level needed to be logged</param>
        public FileLogger(int maxRecords, string pathFormat, Level minLevel, Level maxLevel)
        {
            if (maxLevel < minLevel)
                throw new ArgumentOutOfRangeException(nameof(maxLevel), "The log levels are not consistent");

            _minLevel = minLevel;
            _maxLevel = maxLevel;
            _queue = new SelfFlushedQueue<LogRecord>(maxRecords,
                lrs =>
                {
                    try
                    {
                        string path = string.Format(pathFormat, DateTime.Now);
                        File.AppendAllLines(path, lrs.Select(lr => lr.ToString()));
                    }
                    catch { }
                });
        }

        /// <summary>Adds a record to the logger</summary>
        /// <param name="record">the <c>LogRecord</c> to add</param>
        public void Add(LogRecord record)
        {
            if (record.Level >= _minLevel && record.Level <= _maxLevel)
                _queue.Add(record);
        }

        /// <summary>Adds a debug line to the log</summary>
        /// <param name="context">the logging context</param>
        /// <param name="message">the message</param>
        public void Debug(string context, string message)
        {
            Add(new LogRecord(context, Level.Debug, message));
        }

        /// <summary>Adds an information line to the log</summary>
        /// <param name="context">the logging context</param>
        /// <param name="message">the message</param>
        public void Info(string context, string message)
        {
            Add(new LogRecord(context, Level.Info, message));
        }

        /// <summary>Adds a warning line to the log</summary>
        /// <param name="context">the logging context</param>
        /// <param name="message">the message</param>
        public void Warning(string context, string message)
        {
            Add(new LogRecord(context, Level.Warn, message));
        }

        /// <summary>Adds a error line to the log</summary>
        /// <param name="context">the logging context</param>
        /// <param name="message">the message</param>
        public void Error(string context, string message)
        {
            Add(new LogRecord(context, Level.Error, message));
        }

        /// <summary>Adds a fatal line to the log</summary>
        /// <param name="context">the logging context</param>
        /// <param name="message">the message</param>
        public void Fatal(string context, string message)
        {
            Add(new LogRecord(context, Level.Fatal, message));
        }
    }
}
