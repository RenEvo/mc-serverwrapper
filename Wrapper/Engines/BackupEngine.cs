using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;

namespace Wrapper.Engines
{
    /// <summary>
    /// Class Backups
    /// </summary>
    public class BackupEngine : IBackupEngine
    {
        /// <summary>
        /// The _logger
        /// </summary>
        private readonly ILogEngine _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="BackupEngine"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <exception cref="System.ArgumentNullException">logger</exception>
        public BackupEngine(ILogEngine logger)
        {
            if (logger == null) throw new ArgumentNullException("logger");
            _logger = logger;
        }

        /// <summary>
        /// Backups the level.
        /// </summary>
        /// <param name="backupDirectory">The backup directory.</param>
        /// <param name="serverPath">The server path.</param>
        public void BackupLevel(string backupDirectory, string serverPath)
        {
            Directory.CreateDirectory(backupDirectory);

            if (!File.Exists(Path.Combine(serverPath, "server.properties")))
                return;

            var serverProperties = File.ReadAllLines(Path.Combine(serverPath, "server.properties"));
            var levelName = "world";

            foreach (var property in serverProperties)
            {
                var kvPair = property.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);

                if (kvPair[0].ToLower() == "level-name")
                    levelName = kvPair.Length > 1 ? kvPair[1].Trim() : levelName;
            }

            if (string.IsNullOrWhiteSpace(levelName) || !Directory.Exists(Path.Combine(serverPath, levelName)))
                return;

            var backupFileName = Path.Combine(backupDirectory, string.Format("{0}.{1}.zip", levelName, DateTime.Now.ToString("yyyy.MM.dd.HH.mm.ss")));

            _logger.Write(TraceEventType.Information, "Back up level starting: {0} to {1}", levelName, backupFileName);
            ZipFile.CreateFromDirectory(Path.Combine(serverPath, levelName), backupFileName);
            _logger.Write(TraceEventType.Information, "Back up level complete: {0} to {1}", levelName, backupFileName);
        }

        /// <summary>
        /// Backups the server.
        /// </summary>
        /// <param name="backupDirectory">The backup directory.</param>
        /// <param name="serverPath">The server path.</param>
        public void BackupServer(string backupDirectory, string serverPath)
        {
            Directory.CreateDirectory(backupDirectory);

            var backupFileName = Path.Combine(backupDirectory, string.Format("Server.Backup.{0}.zip", DateTime.Now.ToString("yyyy.MM.dd.HH.mm.ss")));

            CleanupDirectory(serverPath);

            _logger.Write(TraceEventType.Information, "Back up server starting: {0}", backupFileName);
            ZipFile.CreateFromDirectory(serverPath, backupFileName);
            _logger.Write(TraceEventType.Information, "Back up server complete: {0}", backupFileName);
        }

        /// <summary>
        /// Cleanups the directory.
        /// </summary>
        /// <param name="serverPath">The server path.</param>
        public void CleanupDirectory(string serverPath)
        {
            // cleanup
            foreach (var file in Directory.GetFiles(serverPath, "*.log"))
                File.Delete(file);

            foreach (var file in Directory.GetFiles(serverPath, "*.log.lck"))
                File.Delete(file);

            if (Directory.Exists(Path.Combine(serverPath, "crash-reports")))
                Directory.Delete(Path.Combine(serverPath, "crash-reports"), true);
            // end cleanup
        }
    }
}
