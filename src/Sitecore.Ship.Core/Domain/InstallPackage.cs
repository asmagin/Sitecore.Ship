using System;
using System.IO;
using System.Security.Cryptography;

namespace Sitecore.Ship.Core.Domain
{
    public class InstallPackage
    {
        public string Path { get; }

        public bool DisableIndexing { get; }

        public string Hash { get; }

        public bool ForceInstall { get; }

        /// <summary>
        /// Set to true to disable reporting of items contained in the package.
        /// </summary>
        public bool DisableManifest { get; set; }

        public InstallPackage()
        {
        }

        public InstallPackage(string path, string disableIndexing = "true", string forceInstall = null)
        {
            Path = path;
            DisableIndexing = disableIndexing != null && disableIndexing.ToLowerInvariant() == "true";
            ForceInstall = forceInstall != null && forceInstall.ToLowerInvariant() == "true";
        }

        public InstallPackage(string path, bool disableIndexing = true, bool forceInstall = false)
        {
            Path = path;
            DisableIndexing = disableIndexing;
            ForceInstall = forceInstall;
        }

        private static string CalculateMD5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }
    }
}