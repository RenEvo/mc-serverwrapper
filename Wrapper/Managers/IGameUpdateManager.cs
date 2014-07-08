using System;
namespace Wrapper.Managers
{
    /// <summary>
    /// Interface IGameUpdateManager
    /// </summary>
    public interface IGameUpdateManager
    {
        /// <summary>
        /// Creates the mod pack.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <param name="serverPath">The server path.</param>
        /// <param name="rootPath">The root path.</param>
        /// <returns>System.String.</returns>
        string CreateModPack(string version, string serverPath, string rootPath);

        /// <summary>
        /// Updates the game.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <param name="serverPath">The server path.</param>
        /// <param name="rootPath">The root path.</param>
        /// <param name="backup">if set to <c>true</c> [backup].</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
        bool UpdateGame(string version, string serverPath, string rootPath, bool backup);
    }
}
