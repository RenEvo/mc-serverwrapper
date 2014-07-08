using System;

namespace Wrapper.Managers
{
    /// <summary>
    /// Class NullServerManager
    /// </summary>
    public class NullServerManager : IServerManager
    {
        /// <summary>
        /// The _player manager
        /// </summary>
        private readonly IPlayerManager _playerManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="NullServerManager" /> class.
        /// </summary>
        /// <param name="playerManager">The player manager.</param>
        /// <exception cref="System.ArgumentNullException">playerManager</exception>
        public NullServerManager(IPlayerManager playerManager)
        {
            if (playerManager == null) throw new ArgumentNullException("playerManager");
            _playerManager = playerManager;
        }

        /// <summary>
        /// Backups the level.
        /// </summary>
        public void BackupLevel()
        {
            
        }

        /// <summary>
        /// Backups the server.
        /// </summary>
        public void BackupServer()
        {
            
        }

        /// <summary>
        /// Compiles the mod pack.
        /// </summary>
        public void CompileModPack()
        {
            
        }

        /// <summary>
        /// Executes the server command.
        /// </summary>
        /// <param name="command">The command.</param>
        public void ExecuteServerCommand(string command)
        {
            
        }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        public void Start()
        {
            _playerManager.Connect("[INFO] [Minecraft-Server] RenEvo[/127.0.0.1:18306] logged in with entity id 281 at (-174.91650710252642, 76.0, 219.1265211316589)");
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        public void Stop()
        {
            _playerManager.Disconnect("[INFO] [STDOUT] Unloading Player: RenEvo");
        }

        /// <summary>
        /// Updates this instance.
        /// </summary>
        /// <param name="backup">if set to <c>true</c> [backup].</param>
        public void Update(bool backup)
        {
            
        }
    }
}
