
namespace Euclid.Logging
{
    /// <summary>
    /// Generic type defining result. The output might be successful or not. 
    /// The main purpose of this class is to avoid throwing exceptions.
    /// </summary>
    /// <typeparam name="T">The type returned by the result output.</typeparam>
    public sealed class ResultOutput<T>
    {
        #region vars
        /// <summary>
        /// Has the process succeeded ?
        /// </summary>
        public bool Succeeded { get; private set; }
        /// <summary>
        /// Has the process failed ?
        /// </summary>
        public bool Failed { get { return !Succeeded; } }
        /// <summary>
        /// Gets the message of the result output ?
        /// </summary>
        public string Message { get; private set; }
        /// <summary>
        /// Gets the value associated to the ResultOutput.
        /// </summary>
        public T Result { get; private set; }
        #endregion

        #region constructor
        /// <summary>
        /// Constructs a new ResultOutput.
        /// </summary>
        /// <param name="succeeded">Has the process succeeded.</param>
        /// <param name="message">The message associated to the result output.</param>
        /// <param name="result">Value held by the result ouptut.</param>
        private ResultOutput(bool succeeded, string message, T result) { Succeeded = succeeded; Message = message; Result = result; }
        #endregion

        #region method(s)
        /// <summary>
        /// Create a result output
        /// </summary>
        /// <param name="succeeded">Has the process succeeded.</param>
        /// <param name="message">The message associated to the result output.</param>
        /// <param name="result">Value held by the result ouptut.</param>
        /// <returns>Result output</returns>
        public static ResultOutput<T> Create(bool succeeded, string message, T result) { return new ResultOutput<T>(succeeded, message, result); }
        /// <summary>
        /// Create a result output for a process which does not succeeded
        /// </summary>
        /// <param name="message">Error message</param>
        /// <returns>Result output</returns>
        public static ResultOutput<T> CreateFailed(string message)
        {
            return Create(false, message, default(T));
        }
        /// <summary>
        /// Implicit constructor of a ResultOutput from a result (cannot be a string).
        /// Builds a succeeded ResultOutput.
        /// </summary>
        /// <param name="result">The result held by the ResultOutput.</param>
        /// <returns>The new ResultOutput.</returns>
        public static implicit operator ResultOutput<T>(T result)
        {
            return Create(true, string.Empty, result);
        }

        #endregion

    }
}
