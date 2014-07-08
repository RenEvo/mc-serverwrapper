using System;
using System.Diagnostics;

namespace Wrapper.Diagnostics
{
    /// <summary>
    /// Implementation of a colored console writer
    /// </summary>
    public class ColorConsoleTraceListener : ConsoleTraceListener
    {
        private static readonly object _consoleLock = new object();

        /// <summary>
        /// Traces an event to the console setting the color by event type
        /// </summary>
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id)
        {
            lock (_consoleLock)
            {
                SetConsoleColor(eventType);
                base.TraceEvent(eventCache, source, eventType, id);
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Traces an event to the console setting the color by event type
        /// </summary>
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
        {
            lock (_consoleLock)
            {
                SetConsoleColor(eventType);
                base.TraceEvent(eventCache, source, eventType, id, format, args);
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Traces an event to the console setting the color by event type
        /// </summary>
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            lock (_consoleLock)
            {
                SetConsoleColor(eventType);
                base.TraceEvent(eventCache, source, eventType, id, message);
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Traces data to the console setting the color by event type
        /// </summary>
        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
        {
            lock (_consoleLock)
            {
                SetConsoleColor(eventType);
                base.TraceData(eventCache, source, eventType, id, data);
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Traces data to the console setting the color by event type
        /// </summary>
        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, params object[] data)
        {
            lock (_consoleLock)
            {
                SetConsoleColor(eventType);
                base.TraceData(eventCache, source, eventType, id, data);
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Determines the color based on event type for the console
        /// </summary>
        private static void SetConsoleColor(TraceEventType eventType)
        {
            switch (eventType)
            {
                case TraceEventType.Information:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case TraceEventType.Verbose:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case TraceEventType.Critical:
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    break;
                case TraceEventType.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case TraceEventType.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case TraceEventType.Start:
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    break;
                case TraceEventType.Stop:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.DarkBlue;
                    break;
            }
        }
    }
}
