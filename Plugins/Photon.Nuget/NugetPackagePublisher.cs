using Photon.Framework.Tools;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.NuGetPlugin
{
    public class NuGetPackagePublisher
    {
        private readonly NuGetCore client;

        public string ExeFilename {get; set;}
        public string PackageId {get; set;}
        public string Version {get; set;}
        public string PackageDirectory {get; set;}
        public string PackageDefinition {get; set;}
        public string Configuration {get; set;}
        public string Platform {get; set;}


        public NuGetPackagePublisher(NuGetCore client)
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

            if (!VersionTools.HasUpdates(packageVersion, Version)) {
                client.Output?
                    .Append($"Package '{PackageId}' is up-to-date. Version ", ConsoleColor.DarkBlue)
                    .AppendLine(packageVersion, ConsoleColor.Blue);

                return;
            }

            var cl = new NuGetCommandLine {
                ExeFilename = ExeFilename,
                ApiKey = client.ApiKey,
                Output = client.Output,
            };

            cl.Pack(PackageDefinition, PackageDirectory);

            var packageFilename = Directory
                .GetFiles(PackageDirectory, $"{PackageId}.*.nupkg")
                .FirstOrDefault();

            await client.PushAsync(packageFilename, token);
        }
    }
}
