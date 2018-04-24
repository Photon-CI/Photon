using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.NuGetPlugin
{
    public class NuGetPackagePublisher
    {
        private readonly NuGetTools client;

        public string PackageId {get; set;}
        public string Version {get; set;}
        public string PackageDirectory {get; set;}
        public string PackageDefinition {get; set;}
        public string Configuration {get; set;}
        public string Platform {get; set;}


        public NuGetPackagePublisher(NuGetTools client)
        {
            this.client = client;

            Configuration = "Release";
            Platform = "AnyCPU";
        }

        public async Task PublishAsync(CancellationToken token)
        {
            client.Output?
                .Append("Checking Package ", ConsoleColor.DarkCyan)
                .Append(PackageId, ConsoleColor.Cyan)
                .AppendLine(" for updates...", ConsoleColor.DarkCyan);

            var versionList = await client.GetAllPackageVersions(PackageId, token);
            var packageVersion = versionList.Any() ? versionList.Max() : null;

            if (!HasUpdates(packageVersion, Version)) {
                client.Output?
                    .Append($"Package '{PackageId}' is up-to-date. Version ", ConsoleColor.DarkBlue)
                    .AppendLine(packageVersion, ConsoleColor.Blue);

                return;
            }

            var packageFile = Path.Combine(PackageDirectory, $"{PackageId}.{Version}.nupkg");

            client.Pack(PackageDefinition, packageFile);

            await client.PushAsync(packageFile, token);
        }

        private static bool HasUpdates(string currentVersion, string newVersion)
        {
            if (string.IsNullOrEmpty(currentVersion)) return true;

            var _current = new Version(currentVersion);
            var _new = new Version(newVersion);

            return _new > _current;
        }
    }
}
