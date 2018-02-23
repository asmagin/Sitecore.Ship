using System;
using System.Collections.Generic;

using Moq;
using Should;
using Xunit;

using Sitecore.Ship.Core.Contracts;
using Sitecore.Ship.Core.Domain;
using Sitecore.Ship.Core.Services;

namespace Sitecore.Ship.Test
{
    public class InstallationRecorderTests
    {
        private readonly Mock<IPackageHistoryRepository> _mockPackageHistoryRepository;
        private readonly PackageInstallationSettings _packageInstallationSettings;
        private readonly InstallationRecorder _recorder;
        private readonly DateTime _dateInstalled;

        public InstallationRecorderTests()
        {
            _mockPackageHistoryRepository = new Mock<IPackageHistoryRepository>();

            _packageInstallationSettings = new PackageInstallationSettings
            {
                RecordInstallationHistory = true
            };

            _recorder = new InstallationRecorder(_mockPackageHistoryRepository.Object, _packageInstallationSettings);

            _dateInstalled = DateTime.Now;
        }

        [Fact]
        public void Record_install_does_not_install_when_disabled()
        {
            _packageInstallationSettings.RecordInstallationHistory = false;

            _recorder.RecordInstall("01-Description.zip", _dateInstalled, "SOMEHASHVALUE");

            _mockPackageHistoryRepository.Verify(x => x.Add(It.IsAny<InstalledPackage>()), Times.Never());
        }

        [Fact]
        public void Record_install_parses_packageid_correctly_when_relative_path_provided()
        {
            _recorder.RecordInstall("01-Description.zip", _dateInstalled, "SOMEHASHVALUE");

            _mockPackageHistoryRepository.Verify(x => x.Add(It.Is<InstalledPackage>(p => p.PackageId == "01")));
        }

        [Fact]
        public void Record_install_parses_packageid_correctly_when_full_path_provided()
        {
            _recorder.RecordInstall("C:\\aaa\\bbb\\01-Description.zip", _dateInstalled, "SOMEHASHVALUE");

            _mockPackageHistoryRepository.Verify(x => x.Add(It.Is<InstalledPackage>(p => p.PackageId == "01")));
        }

        [Fact]
        public void Record_install_parses_description_correctly()
        {
            _recorder.RecordInstall("01-Description.zip", _dateInstalled, "SOMEHASHVALUE");

            _mockPackageHistoryRepository.Verify(x => x.Add(It.Is<InstalledPackage>(p => p.Description == "Description")));
        }

        [Fact]
        public void Recorder_returns_highest_version_from_get_latest_when_multiple_entries_exist()
        {
            _mockPackageHistoryRepository.Setup(x => x.GetAll()).Returns(
                new List<InstalledPackage>
                    {
                        new InstalledPackage("01", DateTime.Now.AddDays(-3), "TestPackage#1", "SOMEHASHVALUE#1"),
                        new InstalledPackage("02", DateTime.Now.AddDays(-2), "TestPackage#2", "SOMEHASHVALUE#2"),
                        new InstalledPackage("03", DateTime.Now.AddDays(-1), "TestPackage#3", "SOMEHASHVALUE#3")
                    });

            var result = _recorder.GetLatestPackage();

            result.PackageId.ShouldEqual("03");
        }
    }
}
