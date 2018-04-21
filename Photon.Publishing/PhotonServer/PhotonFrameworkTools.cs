using Photon.Framework.Agent;
using Photon.Framework.Tools;
using Photon.NuGetPlugin;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Publishing.PhotonServer
{
    internal class PhotonFrameworkTools
    {
        private const string PackageId = "Photon.Framework";

        private readonly NuGetTools client;
        public IAgentBuildContext Context {get;}


        public PhotonFrameworkTools(IAgentBuildContext context)
        {
            this.Context = context;

            client = new NuGetTools(context);
        }

        public async Task Publish(CancellationToken token)
        {
            var currentVersion = GetCurrentVersion();
            var latestVersion = await GetLatestVersionAsync(token);

            if (currentVersion == null)
                throw new ApplicationException("Unable to determine version of current assembly!");

            if (latestVersion == null)
                Context.Output.AppendLine($"No versions of '{PackageId}' were found on the NuGet server!", ConsoleColor.DarkYellow);

            if (currentVersion <= latestVersion) {
                Context.Output.AppendLine($"Package '{PackageId}' is up-to-date.", ConsoleColor.DarkCyan);
                return;
            }

            await PushPackage(currentVersion, token);
        }

        private string GetCurrentVersion()
        {
            var binDirectory = Path.Combine(Context.ContentDirectory, "Photon.Framework", "bin", "Release");
            var assemblyFilename = Path.Combine(binDirectory, "Photon.Framework.dll");
            return AssemblyTools.GetVersion(assemblyFilename);
        }

        private async Task<Version> GetLatestVersionAsync(CancellationToken token)
        {
            var versionList = await client.GetAllVersions(PackageId, token);
            return versionList.Any() ? versionList.Max() : null;
        }

        private async Task PushPackage(Version version, CancellationToken token)
        {
            var nuspecFilename = Path.Combine(Context.ContentDirectory, "Photon.Framework", "Photon.Framework.nuspec");
            var packageFilename = Path.Combine(Context.WorkDirectory, $"Photon.Framework.{version}.nupkg");

            client.Pack(nuspecFilename, packageFilename);

            var globalVars = Context.ServerVariables.Global;
            var apiKey = (Func<string, string>)(x => globalVars.GetValue<string>("nuget.apiKey"));

            await client.PushAsync(packageFilename, apiKey, token);
        }
    }
}
