using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.SecurityModel;
using Sitecore.Ship.Core.Contracts;
using Sitecore.Ship.Core.Domain;

namespace Sitecore.Ship.Infrastructure.DataAccess
{
    public class PackageHistoryRepository : IPackageHistoryRepository
    {
        private const string PACKAGE_HISTORY_TEMPLATE_PATH = "SitecoreShip/InstalledPackage";

        private const string DATABASE_CORE = "core";

        private const string PACKAGE_ID_FIELD_NAME = "PackageId";
        private const string DATE_INSTALLED_FIELD_NAME = "DateInstalled";
        private const string DESCRIPTION_FIELD_NAME = "Description";
        private const string HASH_FIELD_NAME = "Hash";

        private static Database _database;
        private static Item _rootItem;

        public void Add(InstalledPackage package)
        {
            var rootItem = GetRootItem();

            using (new SecurityDisabler())
            {
                var name = Path.GetFileNameWithoutExtension(package.Description).Replace(".", string.Empty);
                TemplateItem template = GetDatabase().GetTemplate(PACKAGE_HISTORY_TEMPLATE_PATH);
                var item = rootItem.Add(String.Format("{0} {1}", package.PackageId, name), template);

                try
                {
                    item.Editing.BeginEdit();
                    item.Fields[PACKAGE_ID_FIELD_NAME].Value = package.PackageId;
                    item.Fields[DATE_INSTALLED_FIELD_NAME].Value = package.DateInstalled.ToString(CultureInfo.InvariantCulture);
                    item.Fields[DESCRIPTION_FIELD_NAME].Value = package.Description;
                    item.Fields[HASH_FIELD_NAME].Value = package.Hash;
                }
                finally
                {
                    item.Editing.EndEdit();
                }
            }
        }

        protected static Database GetDatabase()
        {
            // master is not available on CD instances, using CORE
            if (_database == null)
            {
                _database = Factory.GetDatabase(DATABASE_CORE);
            }

            return _database;
        }

        public List<InstalledPackage> GetAll()
        {
            var entries = new List<InstalledPackage>();

            var rootItem = GetRootItem();

            if (rootItem != null)
            {
                foreach (Item child in rootItem.Children)
                {
                    entries.Add(new InstalledPackage(
                        child.Fields[PACKAGE_ID_FIELD_NAME].Value,
                        DateTime.Parse(child.Fields[DATE_INSTALLED_FIELD_NAME].Value),
                        child.Fields[DESCRIPTION_FIELD_NAME].Value,
                        child.Fields[HASH_FIELD_NAME].Value
                    ));
                }
            }

            return entries;
        }

        public bool IsAlreadyInstalled(string packageName, string hash, bool forceInstall)
        {
            var rootItem = GetRootItem();

            if (rootItem != null && string.IsNullOrEmpty(packageName) == false && forceInstall == false)
            {
                foreach (Item child in rootItem.Children)
                {
                    var curPackageName = child.Fields[DESCRIPTION_FIELD_NAME].Value;
                    var packageHash = child.Fields[HASH_FIELD_NAME].Value;
                    if (packageName.Equals(curPackageName, StringComparison.InvariantCultureIgnoreCase) &&
                        hash.Equals(packageHash, StringComparison.InvariantCultureIgnoreCase)) return true;
                }
            }

            return false;
        }

        private static Item GetRootItem()
        {
            if (_rootItem == null)
            {
                string pattern = "[^A-Za-z0-9]";
                Regex regex = new Regex(pattern);

                var machineName = regex.Replace(Environment.MachineName, string.Empty).ToUpperInvariant();
                var folderPath = $"/sitecore/System/Modules/Ship/PackageHistory/{machineName}";

                var database = GetDatabase();
                var rootItem = database.GetItem(folderPath) ?? CreateDatabaseItems(database, folderPath);
                _rootItem = rootItem;
            }

            return _rootItem;
        }

        private static Item CreateDatabaseItems(Database database, string path)
        {
            const string TEMPLATE_FOLDER_PATH = "/sitecore/templates/SitecoreShip";
            const string SYSTEM_TEMPLATE_FOLDER_PATH = "/sitecore/templates/System/Templates/Template";
            const string TEMPLATE_NAME = "InstalledPackage";
            const string TEMPLATE_ICON = "People/32x32/package.png";
            const string ICON_FIELD_NAME = "__icon";
            const string TEMPLATE_SECTION_NAME = "Data";

            using (new SecurityDisabler())
            {
                var rootItem = database.CreateItemPath(path);
                var templateRoot = database.CreateItemPath(TEMPLATE_FOLDER_PATH);

                var baseTemplate = database.GetItem(SYSTEM_TEMPLATE_FOLDER_PATH);

                var item = templateRoot.Add(TEMPLATE_NAME, new TemplateItem(baseTemplate));
                item.Editing.BeginEdit();
                item.Fields[ICON_FIELD_NAME].Value = TEMPLATE_ICON;
                item.Editing.EndEdit();

                var newTemplate = new TemplateItem(item);
                newTemplate.AddSection(TEMPLATE_SECTION_NAME);
                newTemplate.AddField(PACKAGE_ID_FIELD_NAME, TEMPLATE_SECTION_NAME);
                newTemplate.AddField(DATE_INSTALLED_FIELD_NAME, TEMPLATE_SECTION_NAME);
                newTemplate.AddField(DESCRIPTION_FIELD_NAME, TEMPLATE_SECTION_NAME);
                newTemplate.AddField(HASH_FIELD_NAME, TEMPLATE_SECTION_NAME);

                return rootItem;
            }
        }
    }
}
