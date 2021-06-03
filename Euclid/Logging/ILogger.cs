namespace Euclid.Logging
{
    public interface ILogger
    {
        void Add(LogRecord record);

        void Debug(string context, string message);

        void Info(string context, string message);

        void Warning(string context, string message);

        void Error(string context, string message);

        void Fatal(string context, string message);
    }
}
