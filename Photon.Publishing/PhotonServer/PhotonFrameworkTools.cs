using Photon.Framework.Agent;
using Photon.NuGetPlugin;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Publishing.PhotonServer
{
    internal class PhotonFrameworkTools
    {
        private const string PackageId = "Photon.Framework";

        public IAgentBuildContext Context {get;}


        public PhotonFrameworkTools(IAgentBuildContext context)
        {
            this.Context = context;
        }

        public async Task Publish()
        {
            var currentVersion = GetCurrentVersion();
            var latestVersion = await GetLatestVersionAsync();

            if (currentVersion == null)
                throw new ApplicationException("Unable to determine version of current assembly!");

            if (latestVersion == null)
                Context.Output.AppendLine($"No versions of '{PackageId}' were found on the NuGet server!", ConsoleColor.DarkYellow);

            if (currentVersion <= latestVersion) {
                Context.Output.AppendLine($"Package '{PackageId}' is up-to-date.", ConsoleColor.DarkCyan);
                return;
            }

            await PushPackage();
        }

        private Version GetCurrentVersion()
        {
            var binDirectory = Path.Combine(Context.ContentDirectory, "Photon.Framework", "bin", "Release");
            var assemblyInfoFilename = Path.Combine(binDirectory, "Photon.Framework.dll");
            var assemblyName = AssemblyName.GetAssemblyName(assemblyInfoFilename);
            return assemblyName.Version;
        }

        private async Task<Version> GetLatestVersionAsync()
        {
            var client = new NuGetTools();
            var versionList = await client.GetAllVersions(PackageId, CancellationToken.None);
            return versionList.Max();
        }

        private async Task PushPackage()
        {
            throw new NotImplementedException();
        }
    }
}
