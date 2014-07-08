using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Wrapper.Engines;

namespace Wrapper.Managers
{
    /// <summary>
    /// Class PlayerManager
    /// </summary>
    public class PlayerManager : IPlayerManager
    {
        /// <summary>
        /// The _logger
        /// </summary>
        private readonly ILogEngine _logger;
        /// <summary>
        /// The _user sessions
        /// </summary>
        private readonly ConcurrentDictionary<string, DateTime> _userSessions = new ConcurrentDictionary<string, DateTime>();

        // username[/127.0.0.1:49273] logged in with entity id 5 at (1.2827418111265, 64.0, 1.1255067590686)
        /// <summary>
        /// The _login regex
        /// </summary>
        private static readonly Regex _loginRegex = new Regex(@"(?<user>.\w+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerManager" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <exception cref="System.ArgumentNullException">logger</exception>
        public PlayerManager(ILogEngine logger)
        {
            if (logger == null) throw new ArgumentNullException("logger");
            _logger = logger;
        }

        /// <summary>
        /// Connects the specified log line.
        /// </summary>
        /// <param name="logLine">The log line.</param>
        public void Connect(string logLine)
        {
            // [08:53:48] [Server thread/INFO]: jrnvd[/127.0.0.1:54097] logged in with entity id 763 at (-290.13059933095315, 98.0, 573.2674900952904)
            // [09:17:36] [Server thread/INFO]: ShoeDawg620 joined the game

            logLine = logLine.Substring("[09:17:36] [Server thread/INFO]: ".Length);

            var userName = logLine.Split(new [] { " " }, 2, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();

            if (string.IsNullOrWhiteSpace(userName))
                return;

            _userSessions.TryAdd(userName, DateTime.Now);
            _logger.Write(TraceEventType.Information, "{0} Logged in", userName);
        }

        /// <summary>
        /// Disconnects the specified log line.
        /// </summary>
        /// <param name="logLine">The log line.</param>
        public void Disconnect(string logLine)
        {
            // [08:59:25] [Server thread/INFO]: jrnvd left the game
            logLine = logLine.Substring("[08:59:25] [Server thread/INFO]: ".Length);

            var userName = logLine.Split(new[] { " " }, 2, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();

            if (string.IsNullOrWhiteSpace(userName))
                return;

            // get the user
            DateTime loggedInTime;
            if (!_userSessions.TryRemove(userName, out loggedInTime))
                return;

            var loginDuration = DateTime.Now.Subtract(loggedInTime);

            _logger.Write(TraceEventType.Information, "{0} Logged out: {1} duration", userName, loginDuration.ToFormattedString());

            // temporary flat file storage of player logins
            var playerDirectory = Path.GetFullPath("./Logs/Players/");
            Directory.CreateDirectory(playerDirectory);

            var fileName = Path.Combine(playerDirectory, userName + ".logins");

            try
            {
                File.AppendAllText(fileName, string.Format("[{0}]\t{1}\t{2}\r\n", DateTime.Now, userName, loginDuration.TotalMinutes));
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
            }

        }

        /// <summary>
        /// Gets the online users.
        /// </summary>
        /// <value>The online users.</value>
        public Dictionary<string, TimeSpan> OnlineUsers
        {
            get
            {
                return _userSessions.ToDictionary(x => x.Key.ToLowerInvariant(), x => DateTime.Now.Subtract(x.Value));
            }
        }

        /// <summary>
        /// Gets the whitelist.
        /// </summary>
        /// <value>The whitelist.</value>
        public IEnumerable<string> Whitelist
        {
            get
            {
                var whiteListFile = Path.Combine(Path.GetFullPath("./server/"), "white-list.txt");
                return !File.Exists(whiteListFile)
                    ? new string[0]
                    : File.ReadAllLines(whiteListFile).Where(x => !string.IsNullOrWhiteSpace(x));
            }
        }

        /// <summary>
        /// Gets the player play stats.
        /// </summary>
        /// <param name="playerName">Name of the player.</param>
        /// <returns>Tuple{System.DoubleSystem.Int32DateTime}.</returns>
        public Tuple<double, int, DateTime> GetPlayerPlayStats(string playerName)
        {
            var totalMinutesPlayed = 0D;
            var lastSeen = DateTime.MinValue;
            var loginCount = 0;

            // temporary flat file storage of player logins
            var playerDirectory = Path.GetFullPath("./Logs/Players/");
            Directory.CreateDirectory(playerDirectory);

            var fileName = Path.Combine(playerDirectory, playerName + ".logins");

            if (File.Exists(fileName))
            {
                var logins = File.ReadAllLines(fileName);
                foreach (var login in logins)
                {
                    var sections = login.Split(new[] { "\t" }, StringSplitOptions.RemoveEmptyEntries);
                    if (sections.Length == 3)
                    {
                        double playTime;
                        double.TryParse(sections[2], out playTime);
                        totalMinutesPlayed += playTime;

                        DateTime logoutTime;
                        DateTime.TryParse(sections[0].Substring(1, sections[0].Length - 2), out logoutTime);

                        lastSeen = logoutTime;
                        loginCount++;
                    }
                }
            }

            return new Tuple<double, int, DateTime>(totalMinutesPlayed / 60, loginCount, lastSeen);
        }
    }
}
