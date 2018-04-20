using Photon.Framework;
using Photon.Framework.Agent;
using Photon.Framework.Tasks;
using Photon.Framework.Tools;
using Photon.Publishing.Internal;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Publishing.Tasks
{
    public class PublishTask : IBuildTask
    {
        private string nugetPackageDir;
        private string nugetApiKey;
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
            apiUrl = photonVars["apiUrl"];
            ftpUrl = photonVars["ftp.url"];
            ftpUser = photonVars["ftp.user"];
            ftpPass = photonVars["ftp.pass"];

            await BuildSolution();
            await PublishFramework(token);
            await PublishServer();
            await PublishAgent();
            await PublishCLI();
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

        private async Task PublishFramework(CancellationToken token)
        {
            var assemblyFilename = Path.Combine(Context.ContentDirectory, "Photon.Framework", "bin", "Release", "Photon.Framework.dll");

            var publisher = new NugetPackagePublisher(Context) {
                NugetExe = Context.AgentVariables.Global["nuget.exe"],
                ProjectFile = Path.Combine("Photon.Framework", "Photon.Framework.csproj"),
                AssemblyVersion = AssemblyTools.GetVersion(assemblyFilename),
                PackageId = "photon.framework",
                PackageDirectory = nugetPackageDir,
                ApiKey = nugetApiKey,
            };

            await publisher.PublishAsync(token);
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
    }
}
