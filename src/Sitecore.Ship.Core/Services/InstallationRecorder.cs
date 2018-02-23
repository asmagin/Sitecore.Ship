using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Sitecore.Ship.Core.Contracts;
using Sitecore.Ship.Core.Domain;

namespace Sitecore.Ship.Core.Services
{

    public class InstallationRecorder : IInstallationRecorder
    {
        private const string FORMAT_STRING = "Missing {0} parameter, required as installation is being recorded";

        private readonly IPackageHistoryRepository _packageHistoryRepository;
        private readonly PackageInstallationSettings _packageInstallationSettings;

        public InstallationRecorder(IPackageHistoryRepository packageHistoryRepository, PackageInstallationSettings packageInstallationSettings)
        {
            _packageHistoryRepository = packageHistoryRepository;
            _packageInstallationSettings = packageInstallationSettings;
        }

        public void RecordInstall(string packagePath, DateTime dateInstalled, string hash)
        {
            if (!_packageInstallationSettings.RecordInstallationHistory) return;

            var packageId = GetPackageIdFromName(packagePath);
            var description = GetDescription(packagePath);

            var record = new InstalledPackage(packageId, dateInstalled, description, hash);

            _packageHistoryRepository.Add(record);
        }

        public void RecordInstall(string packageId, DateTime dateInstalled, string hash, string description = null)
        {
            if (!_packageInstallationSettings.RecordInstallationHistory) return;

            if (string.IsNullOrEmpty(packageId)) throw new ArgumentException(string.Format(FORMAT_STRING, "PackageId"));
            if (string.IsNullOrEmpty(description)) throw new ArgumentException(string.Format(FORMAT_STRING, "Description"));
            if (string.IsNullOrEmpty(hash)) throw new ArgumentException(string.Format(FORMAT_STRING, "Hash"));

            var record = new InstalledPackage(packageId, dateInstalled, description, hash);

            _packageHistoryRepository.Add(record);
        }

        public InstalledPackage GetLatestPackage()
        {
            if (_packageInstallationSettings.RecordInstallationHistory)
            {
                var children = _packageHistoryRepository.GetAll();
                var package = children.OrderByDescending(x => int.Parse(x.PackageId)).FirstOrDefault();

                if (package != null) return package;
            }

            return new InstalledPackageNotFound();
        }

        public ICollection<InstalledPackage> GetInstalledPackages()
        {
            if (_packageInstallationSettings.RecordInstallationHistory)
            {
                var children = _packageHistoryRepository.GetAll();
                var packages = children.OrderByDescending(x => int.Parse(x.PackageId)).ToList();

                return packages;
            }

            return new List<InstalledPackage>();
        }

        private string GetDescription(string packagePath)
        {
            return Path.GetFileName(packagePath).Split('-').Last().Split('.').First();
        }

        private string GetPackageIdFromName(string packagePath)
        {
            //Abstract this so can inject own convention?

            //So, Covention is currently: {ID}-DescriptiveName.extension
            // E.G 01-AboutPage.update
            // E.G 02-HomePage.zip
            return Path.GetFileName(packagePath).Split('-').First();
        }
    }
}