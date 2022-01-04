using System;

namespace Euclid.Logging
{
    /// <summary>Log record class</summary>
    public sealed class LogRecord
    {
        #region Variables
        private readonly Level _level;
        private readonly DateTime _timeStamp;
        private readonly string _message, _context;
        #endregion

        /// <summary>Builds a log record</summary>
        /// <param name="context">the logging context</param>
        /// <param name="level">the level</param>
        /// <param name="message">the message</param>
        public LogRecord(string context, Level level, string message)
        {
            _context = context;
            _level = level;
            _timeStamp = DateTime.Now;
            _message = message;
        }

        #region Accessors
        /// <summary>Returns the log's timestamp</summary>
        public DateTime TimeStamp => _timeStamp;

        /// <summary>Returns the level</summary>
        public Level Level => _level;

        /// <summary>Returns the context</summary>
        public string Context => _context;

        /// <summary>Returns the log's message</summary>
        public string Message => _message;
        #endregion

        /// <summary>Serializes the record</summary>
        /// <returns>a <c>String</c></returns>
        public override string ToString()
        {
            return $"[{_context}][{_level}][{_timeStamp:ddMMyy HHmmss}] {_message}"; ;
        }
    }
}
