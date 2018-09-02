using Photon.Framework.Agent;
using Photon.Framework.Packages;
using Photon.Framework.Tasks;
using Photon.MSBuild;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Publishing
{
    public class Publish_Linux : IBuildTask
    {
        private ApplicationPackageUtility appPackages;
        private string packageVersion;
        private string packageDirectory;

        public IAgentBuildContext Context {get; set;}


        public async Task RunAsync(CancellationToken token)
        {
            await BuildSolution(token);

            var d = DateTime.Now;
            packageVersion = $"{d.Year}.{d.Month}.{d.Day}.{Context.BuildNumber}";
            packageDirectory = Path.Combine(Context.ContentDirectory, "packages");

            appPackages = new ApplicationPackageUtility(Context) {
                PackageDirectory = packageDirectory,
            };

            await Task.WhenAll(
                PackageProject(token),
                PackageServer(token),
                PackageAgent(token));
        }

        private async Task BuildSolution(CancellationToken token)
        {
            var msbuild = new MSBuildCommand(Context) {
                Exe = Context.AgentVariables["global"]["msbuild_exe"],
                WorkingDirectory = Context.ContentDirectory,
            };

            var buildArgs = new MSBuildArguments {
                ProjectFile = "Photon.sln",
                Targets = {"Rebuild"},
                Properties = {
                    ["Configuration"] = "Release",
                    ["Platform"] = "Any CPU",
                },
                Verbosity = MSBuildVerbosityLevel.Minimal,
                NodeReuse = false,
                NoLogo = true,
                MaxCpuCount = 0,
            };

            await msbuild.RunAsync(buildArgs, token);
        }

        private async Task PackageProject(CancellationToken token)
        {
            var packageDefinition = Path.Combine(Context.ContentDirectory, "Photon.Publishing", "Photon.Publishing.Linux.json");

            var projectPackages = new ProjectPackageUtility(Context) {
                PackageDirectory = packageDirectory,
            };

            await projectPackages.Publish(packageDefinition, packageVersion, token);
        }

        private async Task PackageServer(CancellationToken token)
        {
            var packageDefinition = Path.Combine(Context.ContentDirectory, "Photon.Server", "Photon.Server.Linux.json");

            await appPackages.Publish(packageDefinition, packageVersion, token);
        }

        private async Task PackageAgent(CancellationToken token)
        {
            var packageDefinition = Path.Combine(Context.ContentDirectory, "Photon.Agent", "Photon.Agent.Linux.json");

            await appPackages.Publish(packageDefinition, packageVersion, token);
        }
    }
}
