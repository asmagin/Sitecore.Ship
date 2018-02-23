using System;
using Sitecore.Ship.Core.Domain;

namespace Sitecore.Ship.Core.Contracts
{
    using System.Collections.Generic;

    public interface IInstallationRecorder
    {
        void RecordInstall(string packageId, DateTime dateInstalled, string hash, string description);
        void RecordInstall(string packagePath, DateTime dateInstalled, string hash);
        InstalledPackage GetLatestPackage();
        ICollection<InstalledPackage> GetInstalledPackages();
    }
}
