namespace Wrapper.Engines
{
    public interface IBackupEngine
    {
        /// <summary>
        /// Backups the level.
        /// </summary>
        /// <param name="backupDirectory">The backup directory.</param>
        /// <param name="serverPath">The server path.</param>
        void BackupLevel(string backupDirectory, string serverPath);

        /// <summary>
        /// Backups the server.
        /// </summary>
        /// <param name="backupDirectory">The backup directory.</param>
        /// <param name="serverPath">The server path.</param>
        void BackupServer(string backupDirectory, string serverPath);

        /// <summary>
        /// Cleanups the directory.
        /// </summary>
        /// <param name="serverPath">The server path.</param>
        void CleanupDirectory(string serverPath);
    }
}