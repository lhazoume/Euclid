using System;
using System.Collections.Generic;
using System.Linq;

namespace Euclid.Logging
{
    /// <summary>Log container class for display, organized for event handling</summary>
    public class LogContainer : ILogger
    {
        private readonly List<LogRecord> _records;
        private readonly Level _minLevel, _maxLevel;
        private EventHandler _dataChanged;

        /// <summary>Builds a log container aimed at catching the records for a given range of levels</summary>
        /// <param name="minLevel">the minimun level</param>
        /// <param name="maxLevel">the maximum level</param>
        public LogContainer(Level minLevel, Level maxLevel)
        {
            if (maxLevel < minLevel)
                throw new ArgumentOutOfRangeException(nameof(maxLevel), "The log levels are not consistent");

            _minLevel = minLevel;
            _maxLevel = maxLevel;
            _records = new List<LogRecord>();
        }

        #region Add logs
        /// <summary>Adds a record to the logger</summary>
        /// <param name="record">the <c>LogRecord</c> to add</param>
        public void Add(LogRecord record)
        {
            if (record.Level >= _minLevel && record.Level <= _maxLevel)
                _records.Add(record);
        }

        /// <summary>Adds a debug record</summary>
        /// <param name="context">the log context</param>
        /// <param name="message">the message</param>
        public void Debug(string context, string message)
        {
            Add(new LogRecord(context, Level.Debug, message));
        }

        /// <summary>Adds an info record</summary>
        /// <param name="context">the log context</param>
        /// <param name="message">the message</param>
        public void Info(string context, string message)
        {
            Add(new LogRecord(context, Level.Info, message));
        }

        /// <summary>Adds a warning record</summary>
        /// <param name="context">the log context</param>
        /// <param name="message">the message</param>
        public void Warning(string context, string message)
        {
            Add(new LogRecord(context, Level.Warn, message));
        }

        /// <summary>Adds an error record</summary>
        /// <param name="context">the log context</param>
        /// <param name="message">the message</param>
        public void Error(string context, string message)
        {
            Add(new LogRecord(context, Level.Error, message));
        }

        /// <summary>Adds a fatal record</summary>
        /// <param name="context">the log context</param>
        /// <param name="message">the message</param>
        public void Fatal(string context, string message)
        {
            Add(new LogRecord(context, Level.Fatal, message));
        }
        #endregion

        #region Handle records
        /// <summary>Returns the logs</summary>
        public List<LogRecord> Records => _records.ToList();

        /// <summary>Clears the log</summary>
        public void Clear()
        {
            if(_records.Count>0)
            {
                _records.Clear();
                FireChangedEvent();
            }
        }

        /// <summary>Removes the logs by the level</summary>
        /// <param name="level">the sought level</param>
        public void Remove(Level level)
        {
            int removed =_records.RemoveAll(r => r.Level == level);
            if (removed > 0) FireChangedEvent();
        }

        /// <summary>Removes the logs by date </summary>
        /// <param name="dateTime">the cut-off date time</param>
        public void RemoveBefore(DateTime dateTime)
        {
            int removed = _records.RemoveAll(r => r.TimeStamp < dateTime);
            if (removed > 0) FireChangedEvent();
        }
        #endregion

        #region Events
        private void FireChangedEvent()
        {
            _dataChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Event handler</summary>
        public event EventHandler DataChanged
        {
            add { _dataChanged += value; }
            remove { _dataChanged -= value; }
        }
        #endregion
    }
}
