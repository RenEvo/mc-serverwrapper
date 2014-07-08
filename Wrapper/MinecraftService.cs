using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using Wrapper.Engines;
using Wrapper.Managers;
using Wrapper.Models;

namespace Wrapper
{
    /// <summary>
    /// Minecraft Server Wrapper service
    /// </summary>
    public partial class MinecraftService : ServiceBase
    {
        private readonly IServerManager _serverManager;
        private readonly IApiEngine _apiEngine;
        private readonly ILogEngine _logger;

        /// <summary>
        /// Default constructor
        /// </summary>
        public MinecraftService(IServerManager serverManager, IApiEngine apiEngine, ILogEngine logger)
        {
            if (serverManager == null) throw new ArgumentNullException("serverManager");
            if (apiEngine == null) throw new ArgumentNullException("apiEngine");
            if (logger == null) throw new ArgumentNullException("logger");
            _serverManager = serverManager;
            _apiEngine = apiEngine;
            _logger = logger;

            InitializeComponent();
        }

        /// <summary>
        /// Internal method to start the service
        /// </summary>
        internal void DebugStart(string[] args)
        {
            OnStart(args);
        }

        /// <summary>
        /// Internal method to stop the service
        /// </summary>
        internal void DebugStop()
        {
            OnStop();
        }

        /// <summary>
        /// Called when the service is started
        /// </summary>
        /// <param name="args">Data passed by the start command.</param>
        /// <exception cref="System.IO.FileNotFoundException"></exception>
        /// <exception cref="System.InvalidOperationException">Invalid memory unit - see java documentation for valid memory settings</exception>
        protected override void OnStart(string[] args)
        {
            base.OnStart(args);

            // verify paths
            if (!File.Exists(MinecraftSettings.Default.JavaPath))
                throw new FileNotFoundException(MinecraftSettings.Default.JavaPath);

            if (MinecraftSettings.Default.Memory % 256 > 0)
                throw new InvalidOperationException("Invalid memory unit - see java documentation for valid memory settings");

            if (args.Length == 0)
            {
                _serverManager.Start();
                _apiEngine.Start();
            }

            else
            {
                foreach (var arg in args)
                {
                    switch (arg.ToLower())
                    {
                        case "-update":
                        case "-u":
                            _serverManager.Update(true);
                            break;

                        case "-updatenobackup":
                        case "-unb":
                            _serverManager.Update(false);
                            break;

                        case "-backuplevel":
                        case "-bl":
                            _serverManager.BackupLevel();
                            break;

                        case "-backup":
                        case "-backupserver":
                        case "-bs":
                            _serverManager.BackupServer();
                            break;

                        case "-compile":
                        case "-c":
                            _serverManager.CompileModPack();
                            break;

                        case "-start":
                        case "-s":
                            _serverManager.Start();
                            _apiEngine.Start();
                            break;

                        case "-install": // TODO: Install Service
                        case "-i":
                            break;

                        case "-uninstall": // TODO: Uninstall Service
                        case "-ui":
                            break;

                        case "-help": // TODO: Help output
                        case "-h":
                        case "-?":
                            break;

                        default:
                            _logger.Write(TraceEventType.Warning, "Unknown Command Line Option: {0}", arg);
                            break;
                    }
                }

            }
        }

        /// <summary>
        /// When implemented in a derived class, executes when a Stop command is sent to the service by the Service Control Manager (SCM). Specifies actions to take when a service stops running.
        /// </summary>
        protected override void OnStop()
        {
            _apiEngine.Stop();

            base.OnStop();

            _serverManager.Stop();
        }

        /// <summary>
        /// Allows for internal classes to execute a custom command, useful for debugging
        /// </summary>
        /// <param name="command">The command.</param>
        internal void ExecuteCustomCommand(int command)
        {
            OnCustomCommand(command);
        }

        /// <summary>
        /// Runs a custom command. up to 128 is reserved by the OS, and 256 is max.
        /// </summary>
        /// <param name="command">The command message sent to the service.</param>
        protected override void OnCustomCommand(int command)
        {
            base.OnCustomCommand(command);

            if (_serverManager == null)
                return;

            if (RunFromConfig(command))
                return;

            switch (command)
            {
                case CustomCommands.Restart:
                    _serverManager.ExecuteServerCommand("stop");
                    break;

                case CustomCommands.DisableSave:
                    _serverManager.ExecuteServerCommand("save-off");
                    break;

                case CustomCommands.EnableSave:
                    _serverManager.ExecuteServerCommand("save-on");
                    break;

                case CustomCommands.ForceSave:
                    _serverManager.ExecuteServerCommand("save-all");
                    break;

                case CustomCommands.BackupLevel:
                    _serverManager.BackupLevel();
                    break;

                case CustomCommands.BackupServer:
                    _serverManager.BackupServer();
                    break;

                case CustomCommands.Update:
                    _serverManager.Update(true);
                    break;

                case CustomCommands.Compile:
                    _serverManager.CompileModPack();
                    break;
            }
        }

        /// <summary>
        /// Allows you to run a command from the config file. app setting key is in the format of Command# and the Value is semi-colon delimited commands.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns>True if it processed a command from the configuration file</returns>
        private bool RunFromConfig(int command)
        {
            if (command > 128 && command < 256 && ConfigurationManager.AppSettings["Command" + command] != null)
            {
                var commands = ConfigurationManager.AppSettings["Command" + command].Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var commandString in commands)
                    _serverManager.ExecuteServerCommand(commandString);

                return true;
            }

            return false;
        }
    }


}
