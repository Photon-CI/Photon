using Photon.Framework;
using Photon.Framework.Agent;
using Photon.Framework.Tasks;
using Photon.Framework.Tools;
using Photon.NuGetPlugin;
using Photon.Publishing.Internal;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Publishing.Tasks
{
    public class PublishTask : IBuildTask
    {
        private NuGetCore nugetClient;
        private string nugetPackageDir;
        private string nugetApiKey;
        private string nugetExe;
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
            nugetApiKey = Context.ServerVariables.Global["nuget.apiKey"];
            nugetExe = Context.AgentVariables.Global["nuget.exe"];
            apiUrl = photonVars["apiUrl"];
            ftpUrl = photonVars["ftp.url"];
            ftpUser = photonVars["ftp.user"];
            ftpPass = photonVars["ftp.pass"];

            nugetClient = new NuGetCore {
                EnableV3 = true,
                Output = Context.Output,
                ApiKey = nugetApiKey,
            };
            nugetClient.Initialize();

            await BuildSolution();
            await PublishServer();
            await PublishAgent();
            await PublishCLI();

            if (!Directory.Exists(nugetPackageDir))
                Directory.CreateDirectory(nugetPackageDir);

            await PublishPluginPackage("Photon.Framework", token);
            await PublishPluginPackage("Photon.IIS", token);
            await PublishPluginPackage("Photon.NuGet", token);
            await PublishPluginPackage("Photon.WindowsServices", token);
            await PublishPluginPackage("Photon.Config", token);
        }

        private async Task BuildSolution()
        {
            await Context.RunCommandLineAsync(
                ".\\bin\\msbuild.cmd", "/m", "/v:m",
                "Photon.sln",
                "/p:Configuration=Release",
                "/p:Platform=\"Any CPU\"",
                "/t:Rebuild");
        }

        private async Task PublishServer()
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

            await publisher.PublishAsync("Photon Server", "Photon.Server");
        }

        private async Task PublishAgent()
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

            await publisher.PublishAsync("Photon Agent", "Photon.Agent");
        }

        private async Task PublishCLI()
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

            await publisher.PublishAsync("Photon CLI", "Photon.CLI");
        }

        private async Task PublishPluginPackage(string id, CancellationToken token)
        {
            var projectPath = Path.Combine(Context.ContentDirectory, "Plugins", id);
            var assemblyFilename = Path.Combine(projectPath, "bin", "Release", $"{id}.dll");

            var publisher = new NuGetPackagePublisher(nugetClient) {
                PackageId = id,
                Version = AssemblyTools.GetVersion(assemblyFilename),
                PackageDirectory = nugetPackageDir,
                PackageDefinition = Path.Combine(projectPath, $"{id}.csproj"),
                ExeFilename = nugetExe,
            };

            await publisher.PublishAsync(token);
        }
    }
}
