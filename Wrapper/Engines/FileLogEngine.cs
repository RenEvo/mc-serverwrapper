using System;
using System.Diagnostics;

namespace Wrapper.Engines
{
    /// <summary>
    /// Class FileLogEngine
    /// </summary>
    public class FileLogEngine : ILogEngine
    {
        /// <summary>
        /// The _logging source
        /// </summary>
        private readonly TraceSource _loggingSource;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileLogEngine"/> class.
        /// </summary>
        public FileLogEngine()
        {
            _loggingSource = new TraceSource("Default", SourceLevels.All);
        }

        /// <summary>
        /// Writes the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="args">The args.</param>
        public void Write(string message, params object[] args)
        {
            Write(TraceEventType.Verbose, message, args);
        }

        /// <summary>
        /// Writes the specified ex.
        /// </summary>
        /// <param name="ex">The ex.</param>
        public void Write(Exception ex)
        {
            Write(TraceEventType.Error, ex.ToString());
        }

        /// <summary>
        /// Writes the specified event type.
        /// </summary>
        /// <param name="eventType">Type of the event.</param>
        /// <param name="message">The message.</param>
        /// <param name="args">The args.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void Write(TraceEventType eventType, string message, params object[] args)
        {
            _loggingSource.TraceEvent(eventType, 0, message, args);
            _loggingSource.Flush();
        }
    }
}
