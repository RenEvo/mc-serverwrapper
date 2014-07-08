using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using NGit;
using NGit.Api;
using Wrapper.Engines;

namespace Wrapper.Managers
{
    /// <summary>
    /// Class GitGameUpdateManager.
    /// </summary>
    public class GitGameUpdateManager : IGameUpdateManager
    {
        /// <summary>
        /// The _backup engine
        /// </summary>
        private readonly IBackupEngine _backupEngine;
        /// <summary>
        /// The _logger
        /// </summary>
        private readonly ILogEngine _logger;

        /// <summary>
        /// The _download location format
        /// </summary>
        private const string _downloadLocationFormat = "https://s3.amazonaws.com/Minecraft.Download/versions/{0}/minecraft_server.{0}.jar";

        /// <summary>
        /// Initializes a new instance of the <see cref="GitGameUpdateManager" /> class.
        /// </summary>
        /// <param name="backupEngine">The backup engine.</param>
        /// <param name="logger">The logger.</param>
        /// <exception cref="System.ArgumentNullException">backupEngine
        /// or
        /// logger</exception>
        public GitGameUpdateManager(IBackupEngine backupEngine, ILogEngine logger)
        {
            if (backupEngine == null) throw new ArgumentNullException("backupEngine");
            if (logger == null) throw new ArgumentNullException("logger");

            _backupEngine = backupEngine;
            _logger = logger;
        }

        /// <summary>
        /// Creates the mod pack.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <param name="serverPath">The server path.</param>
        /// <param name="rootPath">The root path.</param>
        /// <returns>System.String.</returns>
        public string CreateModPack(string version, string serverPath, string rootPath)
        {
            return string.Empty;
        }

        /// <summary>
        /// Updates the game.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <param name="serverPath">The server path.</param>
        /// <param name="rootPath">The root path.</param>
        /// <param name="backup">if set to <c>true</c> [backup].</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
        public bool UpdateGame(string version, string serverPath, string rootPath, bool backup)
        {
            if (backup)
                _backupEngine.BackupServer(Path.Combine(rootPath, "Backups"), serverPath);

            _logger.Write(TraceEventType.Information, "Game Update Starting");

            var currentForgeVersion = Directory.GetFiles(serverPath, "minecraftforge-installer-" + version + "-*.jar").FirstOrDefault();

            var path = Path.GetFullPath(serverPath);

            if (!Directory.Exists(Path.Combine(path, ".git")))
            {
                // CLONE the repository
                _logger.Write("Cloning Modpack from: {0}", MinecraftSettings.Default.ModProviderRoot);

                Directory.CreateDirectory(path);
                var cloneCommand = Git.CloneRepository();
                cloneCommand.SetDirectory(path);
                cloneCommand.SetURI(MinecraftSettings.Default.ModProviderRoot);
                cloneCommand.SetProgressMonitor(new LogProgressMonitor(_logger));

                cloneCommand.Call();
            }
            else
            {
                // reset the repository and pull the latest. We are doing a reset because we have chosen to USE the git repository, local changes are always reverted
                _logger.Write("Resetting local versioned mod pack files");
                Git.Open(path).Reset().SetMode(ResetCommand.ResetType.HARD).Call();

                _logger.Write("Updating Modpack from: {0}", MinecraftSettings.Default.ModProviderRoot);
                var pullCommand = Git.Open(path).Pull();
                pullCommand.SetProgressMonitor(new LogProgressMonitor(_logger));

                var result = pullCommand.Call();
                var messages = result.GetFetchResult().GetMessages();
                if (!string.IsNullOrWhiteSpace(messages))
                    _logger.Write(messages);

                _logger.Write(result.GetMergeResult().GetMergeStatus().ToString());
            }

            var newForgeVersion = Directory.GetFiles(serverPath, "minecraftforge-installer-" + version + "-*.jar").FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(newForgeVersion) && newForgeVersion != currentForgeVersion)
            {
                // update forge
                if (Directory.Exists(Path.Combine(serverPath, "libraries")))
                    Directory.Delete(Path.Combine(serverPath, "libraries"), true);

                var forgeWrapper = Directory.GetFiles(serverPath, "minecraftforge-universal-" + version + "-*.jar").FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(forgeWrapper))
                    File.Delete(forgeWrapper);

                if (File.Exists(Path.Combine(serverPath, "minecraft_server." + version + ".jar")))
                    File.Delete(Path.Combine(serverPath, "minecraft_server." + version + ".jar"));

                var forgeInstallerInfo = new ProcessStartInfo()
                    {
                        Arguments = "--installServer",
                        CreateNoWindow = true,
                        FileName = newForgeVersion,
                        UseShellExecute = true,
                        WorkingDirectory = serverPath,
                    };

                _logger.Write("Updating Forge : {0}", Path.GetFileNameWithoutExtension(newForgeVersion));

                var forgeInstaller = Process.Start(forgeInstallerInfo);

                forgeInstaller.WaitForExit();
                _logger.Write("Forge Update Exited with {0}", forgeInstaller.ExitCode);

                forgeWrapper = Directory.GetFiles(serverPath, "minecraftforge-universal-" + version + "-*.jar").FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(forgeWrapper) && File.Exists(forgeWrapper))
                {
                    _logger.Write("Renaming Forge Launcher");
                    File.Copy(forgeWrapper, Path.Combine(serverPath, "minecraft_server.jar"));
                }

            }
            else if (string.IsNullOrWhiteSpace(currentForgeVersion) && !string.IsNullOrWhiteSpace(newForgeVersion) && !File.Exists(Path.Combine(serverPath, "minecraft_server." + version + ".jar")))
            {
                // install forge
                var forgeInstallerInfo = new ProcessStartInfo()
                {
                    Arguments = "--installServer",
                    CreateNoWindow = true,
                    FileName = newForgeVersion,
                    UseShellExecute = true,
                    WorkingDirectory = serverPath,
                };

                _logger.Write("Installing Forge : {0}", Path.GetFileNameWithoutExtension(newForgeVersion));

                var forgeInstaller = Process.Start(forgeInstallerInfo);

                forgeInstaller.WaitForExit();
                _logger.Write("Forge Install Exited with {0}", forgeInstaller.ExitCode);

                var forgeWrapper = Directory.GetFiles(serverPath, "minecraftforge-universal-" + version + "-*.jar").FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(forgeWrapper) && File.Exists(forgeWrapper))
                {
                    _logger.Write("Renaming Forge Launcher");
                    File.Copy(forgeWrapper, Path.Combine(serverPath, "minecraft_server.jar"));
                }
            }
            else if (!File.Exists(Path.Combine(serverPath, "minecraft_server." + version + ".jar")))
            {
                // download server by its self
                _logger.Write(TraceEventType.Information, "Downloading Minecraft Server");
                new WebClient().DownloadFile(string.Format(_downloadLocationFormat, version), Path.Combine(serverPath, "minecraft_server." + version + ".jar"));
                _logger.Write(TraceEventType.Information, "Downloaded Version {0}", version);
            }
            
            return true;
        }

        /// <summary>
        /// Class LogProgressMonitor.
        /// </summary>
        private class LogProgressMonitor : ProgressMonitor
        {
            /// <summary>
            /// The _logger
            /// </summary>
            private readonly ILogEngine _logger;

            /// <summary>
            /// The _total work
            /// </summary>
            private int _totalWork = 0;
            /// <summary>
            /// The _current work
            /// </summary>
            private int _currentWork = 0;

            /// <summary>
            /// Initializes a new instance of the <see cref="LogProgressMonitor" /> class.
            /// </summary>
            /// <param name="logger">The logger.</param>
            public LogProgressMonitor(ILogEngine logger)
            {
                _logger = logger;
            }

            /// <summary>
            /// Starts the specified total tasks.
            /// </summary>
            /// <param name="totalTasks">The total tasks.</param>
            public override void Start(int totalTasks)
            {
                _logger.Write("Starting {0} Tasks", totalTasks);
            }

            /// <summary>
            /// Begins the task.
            /// </summary>
            /// <param name="title">The title.</param>
            /// <param name="totalWork">The total work.</param>
            public override void BeginTask(string title, int totalWork)
            {
                _currentWork = 0;
                _totalWork = totalWork;

                _logger.Write(title);
            }

            /// <summary>
            /// Updates the specified completed.
            /// </summary>
            /// <param name="completed">The completed.</param>
            public override void Update(int completed)
            {
                _currentWork += completed;
                if (_totalWork > 0)
                    _logger.Write("Step {0} of {1}", _currentWork, _totalWork);
                else
                    _logger.Write("Working on step {0}", _currentWork);
            }

            /// <summary>
            /// Ends the task.
            /// </summary>
            public override void EndTask()
            {
               
            }

            /// <summary>
            /// Determines whether this instance is cancelled.
            /// </summary>
            /// <returns><c>true</c> if this instance is cancelled; otherwise, <c>false</c>.</returns>
            public override bool IsCancelled()
            {
                return false;
            }
        }

    }
}
