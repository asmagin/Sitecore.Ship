using System;
using System.Net;
using System.Web;

using Newtonsoft.Json;
using Sitecore.Ship.Core;
using Sitecore.Ship.Core.Contracts;
using Sitecore.Ship.Core.Domain;
using Sitecore.Ship.Core.Services;
using Sitecore.Ship.Infrastructure.Configuration;
using Sitecore.Ship.Infrastructure.DataAccess;
using Sitecore.Ship.Infrastructure.Web;

namespace Sitecore.Ship.AspNet.Package
{
    public class InstalledPackagesCommand : CommandHandler
    {
        private readonly IInstallationRecorder _installationRecorder;

        public InstalledPackagesCommand(IInstallationRecorder installationRecorder)
        {
            _installationRecorder = installationRecorder;
        }

        public InstalledPackagesCommand() : this(new InstallationRecorder(new PackageHistoryRepository(), new PackageInstallationConfigurationProvider().Settings))
        {
        }

        public override void HandleRequest(HttpContextBase context)
        {
            if (CanHandle(context))
            {
                try
                {
                    var installedPackages = _installationRecorder.GetInstalledPackages();

                    if (installedPackages.Count == 0)
                    {
                        context.Response.StatusCode = (int) HttpStatusCode.NoContent;
                    }
                    else
                    {
                        var json =JsonConvert.SerializeObject(new { installedPackages });

                        JsonResponse(json, HttpStatusCode.OK, context);
                    }
                }
                catch (NotFoundException)
                {
                    context.Response.StatusCode = (int) HttpStatusCode.NotFound;
                }
            }
            else if (Successor != null)
            {
                Successor.HandleRequest(context);
            }
        }

        private static bool CanHandle(HttpContextBase context)
        {
            return context.Request.Url != null &&
                   context.Request.Url.PathAndQuery.EndsWith(ShipServiceUrl.InstalledPackagesCommand, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}