using Photon.Framework.Domain;
using Photon.NuGetPlugin;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Publishing.Internal
{
    internal class NugetPackagePublisher
    {
        private readonly IDomainContext context;
        private readonly NuGetTools client;

        public string NugetExe {get; set;}
        public string PackageId {get; set;}
        public string AssemblyVersion {get; set;}
        public string PackageDirectory {get; set;}
        public string ProjectFile {get; set;}
        public string Configuration {get; set;}
        public string Platform {get; set;}
        public string Source {get; set;}
        public string ApiKey {get; set;}


        public NugetPackagePublisher(IDomainContext context)
        {
            this.context = context;

            NugetExe = "nuget";
            Source = "https://www.nuget.org/api/v2/package";
            Configuration = "Release";
            Platform = "AnyCPU";

            client = new NuGetTools();
            client.EnableV3 = true;
        }

        public async Task PublishAsync(CancellationToken token)
        {
            context.Output
                .Append("Updating Package ", ConsoleColor.DarkCyan)
                .Append(PackageId, ConsoleColor.Cyan)
                .AppendLine("...", ConsoleColor.DarkCyan);

            var versionList = await client.GetAllPackageVersions(PackageId, token);
            var packageVersion = versionList.Any() ? versionList.Max() : null;

            if (!HasUpdates(packageVersion, AssemblyVersion)) {
                context.Output
                    .Append($"Package '{PackageId}' is up-to-date. Version ", ConsoleColor.DarkBlue)
                    .AppendLine(packageVersion, ConsoleColor.Blue);

                return;
            }

            var packageFile = Path.Combine(PackageDirectory, $"{PackageId}.*.nupkg");

            await context.RunCommandLineAsync(NugetExe, "pack",
                $"\"{ProjectFile}\"",
                $"-Prop \"Configuration={Configuration};Platform={Platform}\"",
                $"-OutputDirectory \"{PackageDirectory}\"");

            await context.RunCommandLineAsync(NugetExe, "push",
                $"\"{packageFile}\"",
                $"-Source \"{Source}\"",
                "-NonInteractive",
                $"-ApiKey \"{ApiKey}\"");

            context.Output
                .Append("Package ", ConsoleColor.DarkGreen)
                .Append(PackageId, ConsoleColor.Green)
                .AppendLine(" pushed successfully.", ConsoleColor.DarkGreen);
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
