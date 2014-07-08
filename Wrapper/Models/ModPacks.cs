using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Wrapper.Models
{
    /// <summary>
    /// Class ModPacks
    /// </summary>
    public class ModPacks
    {
        // TODO: Support updating mod information based on a mod.info in the ModPacks directory

        /// <summary>
        /// The _inner document
        /// </summary>
        private XDocument _innerDocument;

        /// <summary>
        /// The _document path
        /// </summary>
        private string _documentPath;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModPacks" /> class.
        /// </summary>
        protected ModPacks()
        {

        }

        /// <summary>
        /// Loads the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>ModPacks.</returns>
        public static ModPacks Load(string path)
        {
            var result = new ModPacks
            {
                _innerDocument = File.Exists(path) ? XDocument.Load(path) : null,
                _documentPath = path,
            };

            return result;
        }

        /// <summary>
        /// Gets the local output directory.
        /// </summary>
        /// <param name="modName">Name of the mod.</param>
        /// <param name="version">The version.</param>
        /// <returns>System.String.</returns>
        public string GetLocalOutputDirectory(string modName, string version)
        {
            if (_innerDocument == null || string.IsNullOrWhiteSpace(modName) || string.IsNullOrWhiteSpace(version))
                return Path.Combine(Path.GetFullPath("./"), "Downloads", modName + "." + version + ".zip");

            XElement modPack = null;
            try
            {
                modPack = _innerDocument.Element("modpacks").Elements("modpack").FirstOrDefault(x => x.Attribute("name").Value == modName);
            }
            catch (Exception)
            {
                // ignore?
                return Path.Combine(Path.GetFullPath("./"), "Downloads", modName + "." + version + ".zip");
            }

            if (modPack == null)
                return Path.Combine(Path.GetFullPath("./"), "Downloads", modName + "." + version + ".zip");

            var modDirectory = modPack.Attribute("dir").Value;
            var modFileName = modPack.Attribute("url").Value;

            return Path.Combine("downloads", "modpacks", modDirectory, version.Replace(".", "_"), modFileName);
        }

        /// <summary>
        /// Sets the version for mod.
        /// </summary>
        /// <param name="modName">Name of the mod.</param>
        /// <param name="version">The version.</param>
        /// <param name="addExistingToOldVersions">if set to <c>true</c> [add existing to old versions].</param>
        public void UpdateMod(string modName, string version, bool addExistingToOldVersions)
        {
            if (_innerDocument == null)
                return;

            XElement modPack = null;
            try
            {
                modPack = _innerDocument.Element("modpacks").Elements("modpack").FirstOrDefault(x => x.Attribute("name").Value == modName);
            }
            catch (Exception)
            {
                // ignore?
            }

            if (modPack == null)
                return;

            var currentVersion = modPack.Attribute("version").Value;
            if (currentVersion == version)
                return;

            modPack.Attribute("version").SetValue(version);

            if (addExistingToOldVersions && !string.IsNullOrWhiteSpace(currentVersion))
            {
                var oldVersions = modPack.Attribute("oldVersions").Value;

                if (!oldVersions.Contains(currentVersion))
                {
                    oldVersions = currentVersion + ";" + oldVersions;
                    modPack.Attribute("oldVersions").SetValue(oldVersions);
                }
            }

            _innerDocument.Save(_documentPath);
        }
    }
}
