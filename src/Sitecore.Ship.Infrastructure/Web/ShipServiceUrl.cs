using System;

namespace Sitecore.Ship.Infrastructure.Web
{
    public class ShipServiceUrl
    {
        public static string PackageLatestVersion
        {
            get { return "/services/package/latestversion"; }
        }

        public static string InstalledPackagesCommand
        {
            get { return "/services/packages/installed"; }
        }
    }
}