using System;

namespace Sitecore.Ship.Core.Domain
{
    public class InstalledPackage
    {
        public string PackageId { get; }
        public DateTime DateInstalled { get; }
        public string Description { get; }
        public string Hash { get; }

        public InstalledPackage(string packageId, DateTime dateInstalled, string description, string hash)
        {
            PackageId = packageId;
            DateInstalled = dateInstalled;
            Description = description;
            Hash = hash;
        }
    }

    public class InstalledPackageNotFound : InstalledPackage
    {
        public InstalledPackageNotFound() :
            base(string.Empty, DateTime.MinValue, string.Empty, "No packages installed or installation recording disabled")
        {
        }
    }
}
