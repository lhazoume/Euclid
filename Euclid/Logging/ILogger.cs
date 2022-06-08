namespace Euclid.Logging
{
    /// <summary>Represents a logger object</summary>
    public interface ILogger
    {
        /// <summary>Adds a log to the logger</summary>
        /// <param name="record">the log record</param>
        void Add(LogRecord record);

        /// <summary>Adds an debug information to the logger</summary>
        /// <param name="context">the debug info's context</param>
        /// <param name="message">the debug info message</param>
        void Debug(string context, string message);

        /// <summary>Adds an information to the logger</summary>
        /// <param name="context">the info's context</param>
        /// <param name="message">the info message</param>
        void Info(string context, string message);

        /// <summary>Adds a warning to the logger</summary>
        /// <param name="context">the warning's context</param>
        /// <param name="message">the warning message</param>
        void Warning(string context, string message);

        /// <summary>Adds an error to the logger</summary>
        /// <param name="context">the error's context</param>
        /// <param name="message">the error message</param>
        void Error(string context, string message);

        /// <summary>Adds a fatal error to the logger</summary>
        /// <param name="context">the fatal error's context</param>
        /// <param name="message">the fatal error's message</param>
        void Fatal(string context, string message);
    }
}
