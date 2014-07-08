namespace Wrapper.Managers
{
    /// <summary>
    /// Interface IServerManager
    /// </summary>
    public interface IServerManager
    {
        /// <summary>
        /// Backups the level.
        /// </summary>
        void BackupLevel();

        /// <summary>
        /// Backups the server.
        /// </summary>
        void BackupServer();

        /// <summary>
        /// Compiles the mod pack.
        /// </summary>
        void CompileModPack();

        /// <summary>
        /// Executes the server command.
        /// </summary>
        /// <param name="command">The command.</param>
        void ExecuteServerCommand(string command);

        /// <summary>
        /// Starts this instance.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops this instance.
        /// </summary>
        void Stop();

        /// <summary>
        /// Updates this instance.
        /// </summary>
        /// <param name="backup">if set to <c>true</c> [backup].</param>
        void Update(bool backup);
    }
}
