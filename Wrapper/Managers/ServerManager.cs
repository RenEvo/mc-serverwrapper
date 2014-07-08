using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Wrapper.Diagnostics;
using Wrapper.Engines;

namespace Wrapper.Managers
{
    /// <summary>
    /// Class ServerManager
    /// </summary>
    public class ServerManager : IServerManager
    {
        /// <summary>
        /// The _player manager
        /// </summary>
        private readonly IPlayerManager _playerManager;
        /// <summary>
        /// The _game update manager
        /// </summary>
        private readonly IGameUpdateManager _gameUpdateManager;
        /// <summary>
        /// The _backup engine
        /// </summary>
        private readonly IBackupEngine _backupEngine;
        /// <summary>
        /// The _logger
        /// </summary>
        private readonly ILogEngine _logger;

        /// <summary>
        /// The _server path
        /// </summary>
        private readonly string _serverPath = string.Empty;

        /// <summary>
        /// The _java path
        /// </summary>
        private readonly string _javaPath = string.Empty;

        /// <summary>
        /// The _version
        /// </summary>
        private readonly string _version = string.Empty;

        /// <summary>
        /// The _memory
        /// </summary>
        private readonly int _memory = 2048;

        private readonly int _permGen = 512;

        /// <summary>
        /// The _is running
        /// </summary>
        private bool _isRunning;

        /// <summary>
        /// The _server process
        /// </summary>
        private Process _serverProcess;

        /// <summary>
        /// The _lag sampler
        /// </summary>
        private readonly PerSecondSampler _lagSampler = new PerSecondSampler();

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerManager"/> class.
        /// </summary>
        /// <param name="playerManager">The player manager.</param>
        /// <param name="gameUpdateManager">The game update manager.</param>
        /// <param name="backupEngine">The backup engine.</param>
        /// <param name="logger">The logger.</param>
        /// <exception cref="System.ArgumentNullException">
        /// playerManager
        /// or
        /// gameUpdateManager
        /// or
        /// backupEngine
        /// or
        /// logger
        /// </exception>
        public ServerManager(IPlayerManager playerManager, IGameUpdateManager gameUpdateManager, IBackupEngine backupEngine, ILogEngine logger)
        {
            if (playerManager == null) throw new ArgumentNullException("playerManager");
            if (gameUpdateManager == null) throw new ArgumentNullException("gameUpdateManager");
            if (backupEngine == null) throw new ArgumentNullException("backupEngine");
            if (logger == null) throw new ArgumentNullException("logger");
            _playerManager = playerManager;
            _gameUpdateManager = gameUpdateManager;
            _backupEngine = backupEngine;
            _logger = logger;

            _javaPath = MinecraftSettings.Default.JavaPath;
            _version = MinecraftSettings.Default.MinecraftVersion;
            _memory = MinecraftSettings.Default.Memory;
            _permGen = MinecraftSettings.Default.PermGen;

            _serverPath = Path.GetFullPath("./Server/");
            Directory.CreateDirectory(_serverPath);
        }

        /// <summary>
        /// Gets the name of the process path.
        /// </summary>
        /// <value>The name of the process path.</value>
        private string ProcessPathName
        {
            get { return Path.Combine(_serverPath, MinecraftSettings.Default.JarName); }
        }

        /// <summary>
        /// Updates the server.
        /// </summary>
        /// <param name="backup">if set to <c>true</c> [backup].</param>
        public void Update(bool backup)
        {
            if (_serverProcess == null)
            {
                _gameUpdateManager.UpdateGame(_version, _serverPath, Path.GetFullPath("./"), backup);
                return;
            }

            Stop();
            // before start, see if we have any updates
            _gameUpdateManager.UpdateGame(_version, _serverPath, Path.GetFullPath("./"), backup);
            Start();
        }

        /// <summary>
        /// Backups the level.
        /// </summary>
        public void BackupLevel()
        {
            if (_serverProcess == null)
            {
                _backupEngine.BackupLevel(Path.Combine(Path.GetFullPath("./"), "Backups"), _serverPath);
                return;
            }

            Stop();
            _backupEngine.BackupLevel(Path.Combine(Path.GetFullPath("./"), "Backups"), _serverPath);
            Start();
        }

        /// <summary>
        /// Backups the server.
        /// </summary>
        public void BackupServer()
        {
            if (_serverProcess == null)
            {
                _backupEngine.BackupServer(Path.Combine(Path.GetFullPath("./"), "Backups"), _serverPath);
                return;
            }

            Stop();
            _backupEngine.BackupServer(Path.Combine(Path.GetFullPath("./"), "Backups"), _serverPath);
            Start();
        }

        /// <summary>
        /// Compiles the mod pack.
        /// </summary>
        public void CompileModPack()
        {
            _gameUpdateManager.CreateModPack(MinecraftSettings.Default.MinecraftVersion, _serverPath, Path.GetFullPath("./"));
        }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        public void Start()
        {
            if (_isRunning)
                return;

            _isRunning = true;

            var startInfo = new ProcessStartInfo(_javaPath, String.Format("-server -XX:+UseConcMarkSweepGC -XX:MaxPermSize={2}M -Xmx{0}M -Xms{0}M -jar \"{1}\" nogui", _memory, ProcessPathName, _permGen))
            {
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = false,
                WorkingDirectory = _serverPath
            };

            _serverProcess = new Process
            {
                StartInfo = startInfo,
                EnableRaisingEvents = true
            };

            _serverProcess.Exited += OnProcessExited;
            _serverProcess.OutputDataReceived += OnStandardDataRecieved;
            _serverProcess.ErrorDataReceived += OnErrorDataRecieved;

            _serverProcess.Start();
            _serverProcess.PriorityClass = ProcessPriorityClass.RealTime;

            // required to read output/error - MC actually writes to the error, which is funny
            _serverProcess.BeginOutputReadLine();
            _serverProcess.BeginErrorReadLine();
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        public void Stop()
        {
            _isRunning = false;

            // tell the player manager to "disconnect" everyone
            _playerManager.DisconnectAll();

            if (_serverProcess == null)
                return;

            if (!_serverProcess.HasExited)
            {
                _serverProcess.StandardInput.WriteLine("stop");
                // could hang, minecraft is finicky that way
                _serverProcess.WaitForExit();
            }

            _serverProcess.Exited -= OnProcessExited;
            _serverProcess.OutputDataReceived -= OnStandardDataRecieved;
            _serverProcess.ErrorDataReceived -= OnErrorDataRecieved;

            _serverProcess.Dispose();
            _serverProcess = null;
        }

        /// <summary>
        /// Executes the server command.
        /// </summary>
        /// <param name="command">The command.</param>
        public void ExecuteServerCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
                return;

            _serverProcess.StandardInput.WriteLine(command);
        }

        /// <summary>
        /// Called when data is available to be read from the Standard Output
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Diagnostics.DataReceivedEventArgs" /> instance containing the event data.</param>
        private void OnStandardDataRecieved(object sender, DataReceivedEventArgs e)
        {
            OnErrorDataRecieved(sender, e);
        }

        /// <summary>
        /// Called when data is available to be read from the Standard Error
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Diagnostics.DataReceivedEventArgs" /> instance containing the event data.</param>
        private void OnErrorDataRecieved(object sender, DataReceivedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(e.Data))
                    return;

                // 1.7.* log lines
                // [08:53:48] [Server thread/INFO]: jrnvd[/77.161.20.40:54097] logged in with entity id 763 at (-290.13059933095315, 98.0, 573.2674900952904)
                // [08:59:25] [Server thread/INFO]: jrnvd left the game

                // this spams the logs, but we should do something if it happens (n) amount of times per minute?
                if (e.Data.Contains("Can't keep up! Did the system time change, or is the server overloaded?"))
                {
                    _lagSampler.Sample();
                    return;
                }

                var logText = e.Data;

                if (e.Data.Length > 11 && e.Data.Substring(11).Contains(":"))
                    logText = e.Data.Substring(11).Substring(e.Data.IndexOf(":", StringComparison.OrdinalIgnoreCase)).Trim();

                // ingame commands
                if (ProcessInGameCommands(logText))
                {
                    _logger.Write(TraceEventType.Information, logText);
                    return;
                }

                if (e.Data.Contains("joined the game"))
                {
                    _playerManager.Connect(e.Data);
                    _logger.Write(TraceEventType.Information, logText);
                }

                if (e.Data.Contains("left the game"))
                {
                    _playerManager.Disconnect(e.Data);
                    _logger.Write(TraceEventType.Information, logText);
                }

                if (e.Data.Contains(" [Server thread/ERROR]:") && e.Data.Contains("crash-reports"))
                {
                    _logger.Write(TraceEventType.Critical, logText);

                    if (!_isRunning)
                        return;

                    // stop and clean up
                    Stop();
                    Start();
                }
            }
            catch (Exception)
            {
                _logger.Write(TraceEventType.Warning, "Unable to read log line: '{0}'", e.Data);
            }
        }

        /// <summary>
        /// Processes the in game commands.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
        private bool ProcessInGameCommands(string data)
        {
            if (data.ToLowerInvariant().Contains("!lag"))
            {
                ExecuteServerCommand(_lagSampler.Rate > 5
                                         ? "/me has detected a bit of server lag. Rate: " + _lagSampler.Rate
                                         : "/me thinks it's you, not me...  Rate: " + _lagSampler.Rate);

                _logger.Write(TraceEventType.Information, "In game Command: {0}", data);
                return true;
            }

            if (data.ToLowerInvariant().Contains("!time"))
            {
                ExecuteServerCommand("/me time is " + DateTime.Now.ToString(CultureInfo.InvariantCulture));

                _logger.Write(TraceEventType.Information, "In game Command: {0}", data);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Called when the watched process is exiting, if it is a non-controlled exit, it will be restarted
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        private void OnProcessExited(object sender, EventArgs e)
        {
            if (!_isRunning)
                return;

            // stop and clean up
            Stop();

            // create a level backup
            _backupEngine.BackupLevel(Path.Combine(Path.GetFullPath("./"), "Backups"), _serverPath);

            // restart the process
            Start();
        }
    }
}
