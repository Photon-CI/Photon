using Photon.Framework;
using Photon.Framework.Agent;
using Photon.Framework.Tasks;
using Photon.Framework.Tools;
using Photon.MSBuildPlugin;
using Photon.NuGet.CorePlugin;
using Photon.NuGetPlugin;
using Photon.Publishing.Internal;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Publishing
{
    public class Publish_Windows : IBuildTask
    {
        private MSBuildCommand msbuild;
        private NuGetCore nugetCore;
        private NuGetCommand nugetCmd;

        private string frameworkVersion;
        private string nugetPackageDir;
        private string apiUrl;
        private string ftpUrl;
        private string ftpUser;
        private string ftpPass;

        public IAgentBuildContext Context {get; set;}


        public async Task RunAsync(CancellationToken token)
        {
            var photonVars = Context.ServerVariables["photon"];

            if (photonVars == null)
                throw new ApplicationException("Photon Variables were not found!");

            nugetPackageDir = Path.Combine(Context.WorkDirectory, "Packages");
            var nugetApiKey = Context.ServerVariables["global"]["nuget/apiKey"];
            apiUrl = photonVars["apiUrl"];
            ftpUrl = photonVars["ftp/url"];
            ftpUser = photonVars["ftp/user"];
            ftpPass = photonVars["ftp/pass"];

            msbuild = new MSBuildCommand(Context) {
                Exe = Context.AgentVariables["global"]["msbuild_exe"],
                WorkingDirectory = Context.ContentDirectory,
            };

            await BuildSolution(token);

            nugetCore = new NuGetCore(Context) {
                ApiKey = nugetApiKey,
            };
            nugetCore.Initialize();

            nugetCmd = new NuGetCommand(Context) {
                Exe = Path.Combine(Context.ContentDirectory, "bin", "NuGet.exe"), //Context.AgentVariables["global"]["nuget_exe"];
                WorkingDirectory = Context.ContentDirectory,
            };

            await Task.WhenAll(
                PublishServer(token),
                PublishAgent(token),
                PublishCLI(token));

            PathEx.CreatePath(nugetPackageDir);

            var projectPath = Path.Combine(Context.ContentDirectory, "Photon.Framework");
            var assemblyFilename = Path.Combine(projectPath, "bin", "Release", "Photon.Framework.dll");
            frameworkVersion = AssemblyTools.GetVersion(assemblyFilename);

            await PublishFrameworkPackage(token);
            await PublishPluginPackage("Photon.MSBuild", token);
            await PublishPluginPackage("Photon.WindowsServices", token);
            await PublishPluginPackage("Photon.Config", token);
            await PublishPluginPackage("Photon.NuGet", token);
            await PublishPluginPackage("Photon.NuGet.Core", token);
            await PublishPluginPackage("Photon.IIS", token);
            await PublishPluginPackage("Photon.NUnit", token);
            await PublishPluginPackage("Photon.DotNet", token);
        }

        private async Task BuildSolution(CancellationToken token)
        {
            await msbuild.RunAsync(new MSBuildArguments {
                ProjectFile = "Photon.sln",
                Targets = {"Rebuild"},
                Properties = {
                    ["Configuration"] = "Release",
                    ["Platform"] = "Any CPU",
                },
                Logger = {
                    ConsoleLoggerParameters = MSBuildConsoleLoggerParameters.Summary
                        | MSBuildConsoleLoggerParameters.ErrorsOnly,
                },
                Verbosity = MSBuildVerbosityLevels.Minimal,
                NodeReuse = false,
                NoLogo = true,
                MaxCpuCount = 0,
            }, token);
        }

        private async Task PublishServer(CancellationToken token)
        {
            var binPath = Path.Combine(Context.ContentDirectory, "Photon.Server", "bin", "Release");

            var publisher = new ApplicationPublisher(Context) {
                VersionUrl = NetPath.Combine(apiUrl, "server/version"),
                UploadPath = NetPath.Combine(ftpUrl, "server"),
                AssemblyFilename = Path.Combine(binPath, "PhotonServer.exe"),
                MsiFilename = Path.Combine(Context.ContentDirectory, "Installers", "Photon.Server.Installer", "bin", "Release", "Photon.Server.Installer.msi"),
                BinPath = binPath,
                PackagePath = nugetPackageDir,
                FtpUsername = ftpUser,
                FtpPassword = ftpPass,
            };

            await publisher.PublishAsync("Photon Server", "Photon.Server", token);
        }

        private async Task PublishAgent(CancellationToken token)
        {
            var binPath = Path.Combine(Context.ContentDirectory, "Photon.Agent", "bin", "Release");

            var publisher = new ApplicationPublisher(Context) {
                VersionUrl = NetPath.Combine(apiUrl, "agent/version"),
                UploadPath = NetPath.Combine(ftpUrl, "agent"),
                AssemblyFilename = Path.Combine(binPath, "PhotonAgent.exe"),
                MsiFilename = Path.Combine(Context.ContentDirectory, "Installers", "Photon.Agent.Installer", "bin", "Release", "Photon.Agent.Installer.msi"),
                BinPath = binPath,
                PackagePath = nugetPackageDir,
                FtpUsername = ftpUser,
                FtpPassword = ftpPass,
            };

            await publisher.PublishAsync("Photon Agent", "Photon.Agent", token);
        }

        private async Task PublishCLI(CancellationToken token)
        {
            var binPath = Path.Combine(Context.ContentDirectory, "Photon.CLI", "bin", "Release");

            var publisher = new ApplicationPublisher(Context) {
                VersionUrl = NetPath.Combine(apiUrl, "cli/version"),
                UploadPath = NetPath.Combine(ftpUrl, "cli"),
                AssemblyFilename = Path.Combine(binPath, "PhotonCLI.exe"),
                MsiFilename = Path.Combine(Context.ContentDirectory, "Installers", "Photon.CLI.Installer", "bin", "Release", "Photon.CLI.Installer.msi"),
                BinPath = binPath,
                PackagePath = nugetPackageDir,
                FtpUsername = ftpUser,
                FtpPassword = ftpPass,
            };

            await publisher.PublishAsync("Photon CLI", "Photon.CLI", token);
        }

        private async Task PublishFrameworkPackage(CancellationToken token)
        {
            var projectPath = Path.Combine(Context.ContentDirectory, "Photon.Framework");
            var packageDefinition = Path.Combine(projectPath, "Photon.Framework.csproj");

            await PublishPackage("Photon.Framework", packageDefinition, frameworkVersion, token);
        }

        private async Task PublishPluginPackage(string id, CancellationToken token)
        {
            var projectPath = Path.Combine(Context.ContentDirectory, "Plugins", id);
            var packageDefinition = Path.Combine(projectPath, $"{id}.csproj");
            var assemblyFilename = Path.Combine(projectPath, "bin", "Release", $"{id}.dll");
            var assemblyVersion = AssemblyTools.GetVersion(assemblyFilename);

            await PublishPackage(id, packageDefinition, assemblyVersion, token);
        }

        private async Task PublishPackage(string packageId, string packageDefinitionFilename, string assemblyVersion, CancellationToken token)
        {
            var versionList = await nugetCore.GetAllPackageVersions(packageId, token);
            var packageVersion = versionList.Any() ? versionList.Max() : null;

            if (!VersionTools.HasUpdates(packageVersion, assemblyVersion)) {
                Context.Output.WriteLine($"Package '{packageId}' is up-to-date. Version {packageVersion}", ConsoleColor.DarkCyan);
                return;
            }

            await nugetCmd.RunAsync(new NuGetPackArguments {
                Filename = packageDefinitionFilename,
                Version = assemblyVersion,
                OutputDirectory = nugetPackageDir,
                Properties = {
                    ["Configuration"] = "Release",
                    ["Platform"] = "AnyCPU",
                    ["frameworkVersion"] = frameworkVersion,
                },
            }, token);

            var packageFilename = Directory
                .GetFiles(nugetPackageDir, $"{packageId}.*.nupkg")
                .FirstOrDefault();

            if (string.IsNullOrEmpty(packageFilename))
                throw new ApplicationException($"No package found matching package ID '{packageId}'!");

            await nugetCore.PushAsync(packageFilename, token);
        }
    }
}
