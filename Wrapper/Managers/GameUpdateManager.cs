using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using Wrapper.Engines;
using Wrapper.Models;

namespace Wrapper.Managers
{
    /// <summary>
    /// Class GameUpdateManager
    /// </summary>
    public class GameUpdateManager : IGameUpdateManager
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
        /// Initializes a new instance of the <see cref="GameUpdateManager"/> class.
        /// </summary>
        /// <param name="backupEngine">The backup engine.</param>
        /// <param name="logger">The logger.</param>
        /// <exception cref="System.ArgumentNullException">
        /// backupEngine
        /// or
        /// logger
        /// </exception>
        public GameUpdateManager(IBackupEngine backupEngine, ILogEngine logger)
        {
            if (backupEngine == null) throw new ArgumentNullException("backupEngine");
            if (logger == null) throw new ArgumentNullException("logger");
            _backupEngine = backupEngine;
            _logger = logger;
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

            var outputLocation = Path.Combine(serverPath, "minecraft_server.jar");

            DownloadServer(version, rootPath);

            var serverFileLocation = CreateModPack(version, serverPath, rootPath);

            CopyModPackDirectory(serverPath, rootPath, "coremods");
            CopyModPackDirectory(serverPath, rootPath, "mods");
            CopyModPackDirectory(serverPath, rootPath, "config");

            if (File.Exists(outputLocation))
                File.Delete(outputLocation);

            _logger.Write("Outputing Update To Server Directory: {0}", outputLocation);
            File.Copy(serverFileLocation, outputLocation, true);

            _logger.Write(TraceEventType.Information, "Game Update Completed");
            return true;
        }

        /// <summary>
        /// Updates the mod pack.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <param name="serverPath">The server path.</param>
        /// <param name="rootPath">The root path.</param>
        /// <returns>System.String.</returns>
        public string CreateModPack(string version, string serverPath, string rootPath)
        {
            _logger.Write(TraceEventType.Information, "Updating Mod Pack");

            var downloadDirectory = Path.Combine(rootPath, "Downloads");
            var modPackLocation = Path.Combine(rootPath, "ModPack");
            var serverFileLocation = Path.Combine(downloadDirectory, string.Format("minecraft_server.{0}.jar", version));

            DownloadServer(version, rootPath);

            // do modpack updatey things
            if (Directory.Exists(modPackLocation))
            {
                if (Directory.Exists(Path.Combine(modPackLocation, "instMods")))
                {
                    _logger.Write(TraceEventType.Information, "Generating Modified Server");

                    var moddedJarName = Path.Combine(downloadDirectory, string.Format("minecraft_server.{0}.modded.jar", version));

                    if (File.Exists(moddedJarName))
                        File.Delete(moddedJarName);

                    File.Copy(serverFileLocation, moddedJarName);

                    _logger.Write(TraceEventType.Information, "Merging Modded Jar File: {0}", moddedJarName);

                    var moddedJar = ZipFile.Open(moddedJarName, ZipArchiveMode.Update);

                    foreach (var file in Directory.GetFiles(Path.Combine(modPackLocation, "instMods"), "*.zip"))
                    {
                        using (var zipFile = ZipFile.OpenRead(file))
                        {
                            foreach (var zippedFile in zipFile.Entries)
                            {
                                Directory.CreateDirectory(Path.GetDirectoryName(Path.Combine(rootPath, "Downloads", version, zippedFile.FullName)) ?? "./ModPack");
                                zippedFile.ExtractToFile(Path.Combine(rootPath, "Downloads", version, zippedFile.FullName), true);

                                var jarEntry = moddedJar.Entries.SingleOrDefault(x => x.FullName == zippedFile.FullName);
                                if (jarEntry != null)
                                {
                                    jarEntry.Delete();
                                }

                                moddedJar.CreateEntryFromFile(Path.Combine(rootPath, "Downloads", version, zippedFile.FullName), zippedFile.FullName);
                            }
                        }
                    }

                    moddedJar.Dispose();

                    // clean up the update directory
                    if (Directory.Exists(Path.Combine(rootPath, "Downloads", version)))
                        Directory.Delete(Path.Combine(rootPath, "Downloads", version), true);

                    serverFileLocation = moddedJarName;
                }

                // TODO: Add support for a URL for the mod provider root
                CreateModPackOutput(rootPath, modPackLocation);
            }

            _logger.Write(TraceEventType.Information, "ModPack Update Completed");

            return serverFileLocation;
        }

        /// <summary>
        /// Downloads the server.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <param name="rootPath">The root path.</param>
        private void DownloadServer(string version, string rootPath)
        {
            _logger.Write(TraceEventType.Information, "Downloading Minecraft Server if Required");

            var downloadDirectory = Path.Combine(rootPath, "Downloads");

            var serverFileLocation = Path.Combine(downloadDirectory, string.Format("minecraft_server.{0}.jar", version));

            Directory.CreateDirectory(downloadDirectory);

            if (!File.Exists(serverFileLocation))
            {
                _logger.Write(TraceEventType.Information, "Downloading Version {0} from {1}", version, string.Format(_downloadLocationFormat, version));
                new WebClient().DownloadFile(string.Format(_downloadLocationFormat, version), serverFileLocation);
                _logger.Write(TraceEventType.Information, "Downloaded Version {0}", version);
            }
        }

        /// <summary>
        /// Creates the mod pack.
        /// </summary>
        /// <param name="rootPath">The root path.</param>
        /// <param name="modPackLocation">The mod pack location.</param>
        private void CreateModPackOutput(string rootPath, string modPackLocation)
        {
            var modPacksFileName = Path.Combine(MinecraftSettings.Default.ModProviderRoot, "static", "FTB2", "modpacks.xml");

            if (!File.Exists(Path.Combine(modPackLocation, "version")))
            {
                File.WriteAllText(Path.Combine(modPackLocation, "version"), MinecraftSettings.Default.MinecraftVersion + ".0");
            }

            // create the version file
            var modPackVersion = File.ReadAllText(Path.Combine(modPackLocation, "version"));
            Version modPackVersionExact;

            if (Version.TryParse(modPackVersion, out modPackVersionExact))
            {
                modPackVersion = string.Format("{0}.{1}.{2}.{3}", modPackVersionExact.Major, modPackVersionExact.Minor, modPackVersionExact.Build, modPackVersionExact.Revision + 1);
                File.WriteAllText(Path.Combine(modPackLocation, "version"), modPackVersion);
            }

            var modPackName = MinecraftSettings.Default.ModPackName;
            var modPackFileName = Path.Combine(rootPath, "Downloads", modPackName + "." + modPackVersion + ".zip");

            // create the zip file
            if (!File.Exists(modPackFileName))
            {
                _logger.Write(TraceEventType.Information, "Creating Mod Pack Output");

                // copy the mod pack to the output directory
                var modPackDirectory = Path.GetDirectoryName(modPackFileName);
                if (modPackDirectory != null)
                    Directory.CreateDirectory(modPackDirectory);

                ZipFile.CreateFromDirectory(modPackLocation, modPackFileName);

                // remove version file (could cause issues with FTB Launcher if we leave this here)
                using (var modPackOutput = ZipFile.Open(modPackFileName, ZipArchiveMode.Update))
                {
                    var entry = modPackOutput.GetEntry("version");
                    entry.Delete();
                }

                _logger.Write(TraceEventType.Information, "ModPack version {2} {0} Compiled to {1}", modPackName, modPackFileName, modPackVersion);
            }

            _logger.Write(TraceEventType.Information, "Mod Packaging Completed");

            if (!File.Exists(modPacksFileName))
                return;

            try
            {
                // Update a modpacks.xml to the new version
                _logger.Write(TraceEventType.Information, "Publishing Mod Pack");

                var modPack = ModPacks.Load(modPacksFileName);

                var outputFile = Path.Combine(MinecraftSettings.Default.ModProviderRoot, modPack.GetLocalOutputDirectory(modPackName, modPackVersion));
                var outputDirectory = Path.GetDirectoryName(outputFile);

                if (outputDirectory != null)
                    Directory.CreateDirectory(outputDirectory);

                File.Copy(modPackFileName, outputFile, true);

                // set the current version of the mod if applicable
                modPack.UpdateMod(modPackName, modPackVersion, true);

                _logger.Write(TraceEventType.Information, "Publishing Complete");
            }
            catch (Exception ex)
            {
                _logger.Write(ex);
            }
        }


        /// <summary>
        /// Copies the mod pack directory.
        /// </summary>
        /// <param name="serverPath">The server path.</param>
        /// <param name="rootPath">The root path.</param>
        /// <param name="directory">The directory.</param>
        private void CopyModPackDirectory(string serverPath, string rootPath, string directory)
        {
            var sourcePath = Path.Combine(rootPath, "ModPack", "minecraft", directory);
            var outputPath = Path.Combine(serverPath, directory);

            if (!Directory.Exists(sourcePath))
                return;

            if (Directory.Exists(outputPath))
                Directory.Delete(outputPath, true);

            Directory.CreateDirectory(outputPath);

            foreach (var file in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                if (IsClientOnly(file))
                {
                    _logger.Write(TraceEventType.Warning, "Ignoring Client Only File: {0}", file);
                    continue;
                }

                var outputFile = file.Replace(sourcePath, outputPath);
                var outputDirectory = Path.GetDirectoryName(outputFile);

                if (string.IsNullOrWhiteSpace(outputFile) || outputDirectory == null)
                    continue;

                Directory.CreateDirectory(outputDirectory);

                _logger.Write("Copying: {0} to {1}", file, outputFile);
                File.Copy(file, outputFile);
            }
        }

        /// <summary>
        /// Determines whether [is client only] [the specified file].
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns><c>true</c> if [is client only] [the specified file]; otherwise, <c>false</c>.</returns>
        private static bool IsClientOnly(string file)
        {
            // TODO: read from config
            return file.ToLower().Contains("inventorytweaks") || file.ToLower().Contains("zansminimap") || file.ToLower().Contains("invtweaks");
        }
    }
}
