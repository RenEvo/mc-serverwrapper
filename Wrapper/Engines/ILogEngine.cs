using System;
using System.Diagnostics;

namespace Wrapper.Engines
{
    /// <summary>
    /// Interface ILogEngine
    /// </summary>
    public interface ILogEngine
    {
        /// <summary>
        /// Writes the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="args">The args.</param>
        void Write(string message, params object[] args);

        /// <summary>
        /// Writes the specified ex.
        /// </summary>
        /// <param name="ex">The ex.</param>
        void Write(Exception ex);

        /// <summary>
        /// Writes the specified event type.
        /// </summary>
        /// <param name="eventType">Type of the event.</param>
        /// <param name="message">The message.</param>
        /// <param name="args">The args.</param>
        void Write(TraceEventType eventType, string message, params object[] args);
    }
}
