// //////////////////////////////////////////////////
// Dependencies
// //////////////////////////////////////////////////

#addin nuget:?package=Cake.Git

// //////////////////////////////////////////////////
// Arguments
// //////////////////////////////////////////////////

var target = Argument("target", "Full");
var config = Argument("config", "Debug");

// //////////////////////////////////////////////////
// Task Definitions
// //////////////////////////////////////////////////
Task("Restore-NuGet-Packages")
    .Description("Restore NuGet packages for solution")
    .Does(() =>
    {
        NuGetRestore("./Sitecore.Ship.sln");
    });

Task("Build-Solution")
    .Description("Build solution with specific parameters")
    .Does(() =>
    {
        var lastCommit = GitLogTip(".");

        MSBuild("./Build/Build.proj", settings => {
              settings.EnvironmentVariables = new Dictionary<string, string>{
                  { "SHA", lastCommit.Sha },
                  { "MinorVersion", "5" },
                  { "TestWebsitePath", "\\\\192.168.50.4\\c$\\inetpub\\wwwroot\\sc90.local" },
                  { "TestWebsiteUrl", "http://tmna.local" },
                  { "LibsSrcPath", "D:\\.projects\\sitecore-engx\\ship\\lib" },
                  { "PRERELEASE", "false"}
              };

              settings.SetConfiguration(config)
                  .UseToolVersion(MSBuildToolVersion.Default)
                  .WithTarget(target)
                  .SetVerbosity(Verbosity.Minimal);
            });

        GitCheckout(".", new FilePath[] { "./src/Common/CommonVersionInfo.cs"});
    });

// //////////////////////////////////////////////////
// Targets
// //////////////////////////////////////////////////
Task("Default")
    .IsDependentOn("Restore-NuGet-Packages")
    .IsDependentOn("Build-Solution");

RunTarget("Default");
