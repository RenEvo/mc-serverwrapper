using System;
using System.Collections.Generic;

namespace Wrapper.Managers
{
    /// <summary>
    /// Interface IPlayerTracker
    /// </summary>
    public interface IPlayerManager
    {
        /// <summary>
        /// Connects the specified log line.
        /// </summary>
        /// <param name="logLine">The log line.</param>
        void Connect(string logLine);

        /// <summary>
        /// Disconnects the specified log line.
        /// </summary>
        /// <param name="logLine">The log line.</param>
        void Disconnect(string logLine);

        /// <summary>
        /// Disconnects all.
        /// </summary>
        void DisconnectAll();

        /// <summary>
        /// Gets the online users.
        /// </summary>
        /// <value>The online users.</value>
        Dictionary<string, TimeSpan> OnlineUsers { get; }

        /// <summary>
        /// Gets the whitelist.
        /// </summary>
        /// <value>The whitelist.</value>
        IEnumerable<string> Whitelist { get; }

        /// <summary>
        /// Gets the player play stats.
        /// </summary>
        /// <param name="playerName">Name of the player.</param>
        /// <returns>Tuple{System.DoubleSystem.Int32DateTime}.</returns>
        Tuple<double, int, DateTime> GetPlayerPlayStats(string playerName);
    }
}
