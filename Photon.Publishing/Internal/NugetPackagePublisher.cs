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

        public string PackageId {get; set;}
        public Version AssemblyVersion {get; set;}
        public string PackageDirectory {get; set;}
        public string ProjectFile {get; set;}
        public string Configuration {get; set;}
        public string Platform {get; set;}
        public string Source {get; set;}


        public NugetPackagePublisher(IDomainContext context)
        {
            this.context = context;

            Source = "https://www.nuget.org/api/v2/package";
            Configuration = "Release";
            Platform = "AnyCPU";

            client = new NuGetTools(context);
        }

        public async Task PublishAsync(CancellationToken token)
        {
            var versionList = await client.GetAllVersions(PackageId, token);
            var packageVersion = versionList.Any() ? versionList.Max() : null;

            if (packageVersion != null && packageVersion >= AssemblyVersion) {
                context.Output
                    .Append($"Package '{PackageId}' is up-to-date. Version ", ConsoleColor.DarkYellow)
                    .AppendLine(packageVersion, ConsoleColor.Yellow);

                return;
            }

            var packageFile = Path.Combine(PackageDirectory, $"{PackageId}.*.nupkg");

            await context.RunCommandLineAsync("nuget", "pack",
                $"\"{ProjectFile}\"",
                $"-Prop \"Configuration={Configuration};Platform={Platform}\"",
                $"-OutputDirectory \"{PackageDirectory}\"");

            await context.RunCommandLineAsync("nuget", "push",
                $"\"{packageFile}\"",
                $"-Source \"{Source}\"",
                "-NonInteractive");
        }
    }
}
