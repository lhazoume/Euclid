using Euclid.DataStructures;
using System;
using System.IO;
using System.Linq;

namespace Euclid.Logging
{
    public class FileLogger : ILogger
    {
        private readonly SelfFlushedQueue<LogRecord> _queue;
        private readonly Level _minLevel, _maxLevel;

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

        public void Debug(string context, string message)
        {
            Add(new LogRecord(context, Level.Debug, message));
        }
        public void Info(string context, string message)
        {
            Add(new LogRecord(context, Level.Info, message));
        }
        public void Warning(string context, string message)
        {
            Add(new LogRecord(context, Level.Warn, message));
        }
        public void Error(string context, string message)
        {
            Add(new LogRecord(context, Level.Error, message));
        }
        public void Fatal(string context, string message)
        {
            Add(new LogRecord(context, Level.Fatal, message));
        }
    }
}
