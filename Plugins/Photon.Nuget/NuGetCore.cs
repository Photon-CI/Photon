using NuGet.Common;
using NuGet.Configuration;
using NuGet.Packaging;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Protocol.Core.v2;
using Photon.Framework.Extensions;
using Photon.Framework.Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.NuGetPlugin
{
    public class NuGetCore
    {
        private SourceRepository sourceRepository;

        public PackageSource PackageSource {get; private set;}
        public string SourceUrl {get; set;}
        public bool EnableV2 {get; set;}
        public bool EnableV3 {get; set;}
        public SourceCacheContext Cache {get; set;}
        public ILogger Logger {get; set;}
        public ScriptOutput Output {get; set;}
        public string ApiKey {get; set;}
        public int PushTimeout {get; set;}


        public NuGetCore()
        {
            SourceUrl = "https://api.nuget.org/v3/index.json";
            PushTimeout = 60;
            Logger = new NullLogger();

            Cache = new SourceCacheContext {
                DirectDownload = true,
                NoCache = true,
            };
        }

        public void Initialize()
        {
            var providers = new List<Lazy<INuGetResourceProvider>>();

            if (EnableV2)
                providers.AddRange(Repository.Provider.GetCoreV2());

            if (EnableV3)
                providers.AddRange(Repository.Provider.GetCoreV3());

            PackageSource = new PackageSource(SourceUrl);
            sourceRepository = new SourceRepository(PackageSource, providers);
        }

        public async Task<string[]> GetAllPackageVersions(string packageId, CancellationToken token)
        {
            var searchResource = await sourceRepository.GetResourceAsync<FindPackageByIdResource>(token);

            if (searchResource == null) throw new ApplicationException("Unable to retrieve package locator resource!");

            var versionList = (await searchResource.GetAllVersionsAsync(packageId, Cache, Logger, token))?.ToArray();

            if (versionList == null) throw new ApplicationException("Unable to retrieve version list!");

            return versionList.Select(x => x.ToString()).ToArray();
        }

        public void Pack(string nuspecFilename, string packageFilename)
        {
            var nuspecName = Path.GetFileName(nuspecFilename);
            var packageName = Path.GetFileName(packageFilename);

            Output?.Append("Parsing package definition ", ConsoleColor.DarkCyan)
                .Append(nuspecName, ConsoleColor.Cyan)
                .AppendLine("...", ConsoleColor.DarkCyan);

            Manifest nuspec;
            try {
                using (var nuspecStream = File.Open(nuspecFilename, FileMode.Open, FileAccess.Read)) {
                    nuspec = Manifest.ReadFrom(nuspecStream, true);
                }
            }
            catch (FileNotFoundException) {
                Output?.Append("Package definition ", ConsoleColor.DarkYellow)
                    .Append(packageName, ConsoleColor.Yellow)
                    .AppendLine(" not found!", ConsoleColor.DarkYellow);

                throw;
            }
            catch (Exception error) {
                Output?.Append("Failed to load package definition ", ConsoleColor.DarkRed)
                    .Append(packageName, ConsoleColor.Red)
                    .AppendLine("!", ConsoleColor.DarkRed)
                    .AppendLine(error.UnfoldMessages(), ConsoleColor.DarkYellow);
                throw;
            }

            Output?.Append("Creating Package ", ConsoleColor.DarkCyan)
                .Append(packageName, ConsoleColor.Cyan)
                .AppendLine("...", ConsoleColor.DarkCyan);

            try {
                var outputPath = Path.GetDirectoryName(packageFilename);

                if (!string.IsNullOrEmpty(outputPath) && !Directory.Exists(outputPath))
                    Directory.CreateDirectory(outputPath);

                var builder = new PackageBuilder();
                builder.Populate(nuspec.Metadata);

                using (var packageStream = File.Open(packageFilename, FileMode.Create, FileAccess.Write)) {
                    builder.Save(packageStream);
                }

                //var path = Path.GetDirectoryName(nuspecFilename);
                //var name = Path.GetFileName(nuspecFilename);

                //var args = string.Join(
                //    "pack", $"\"{name}\"",
                //    $"-OutputDirectory \"{PackageDirectory}\"");

                //ProcessRunner.Run(path, NugetExe, args, Output);

                Output?.Append("Package ", ConsoleColor.DarkGreen)
                    .Append(packageName, ConsoleColor.Green)
                    .AppendLine(" created successfully.", ConsoleColor.DarkGreen);
            }
            catch (Exception error) {
                Output?.Append("Failed to create package ", ConsoleColor.DarkRed)
                    .Append(packageName, ConsoleColor.Red)
                    .AppendLine("!", ConsoleColor.DarkRed)
                    .AppendLine(error.UnfoldMessages(), ConsoleColor.DarkYellow);

                throw;
            }
        }

        public async Task PushAsync(string packageFilename, CancellationToken token)
        {
            var packageName = Path.GetFileName(packageFilename);

            Output?.Append("Publishing Package ", ConsoleColor.DarkCyan)
                .Append(packageName, ConsoleColor.Cyan)
                .AppendLine("...", ConsoleColor.DarkCyan);

            try {
                var apiKeyFunc = (Func<string, string>)(x => ApiKey);
                var updateResource = await sourceRepository.GetResourceAsync<PackageUpdateResource>(token);
                await updateResource.Push(packageFilename, null, PushTimeout, false, apiKeyFunc, null, Logger);

                Output?.Append("Package ", ConsoleColor.DarkGreen)
                    .Append(packageName, ConsoleColor.Green)
                    .AppendLine(" published successfully.", ConsoleColor.DarkGreen);
            }
            catch (Exception error) {
                Output?.Append("Failed to publish package ", ConsoleColor.DarkRed)
                    .Append(packageName, ConsoleColor.Red)
                    .AppendLine("!", ConsoleColor.DarkRed)
                    .AppendLine(error.UnfoldMessages(), ConsoleColor.DarkYellow);

                throw;
            }

            //await context.RunCommandLineAsync(NugetExe, "push",
            //    $"\"{packageFile}\"",
            //    $"-Source \"{Source}\"",
            //    "-NonInteractive",
            //    $"-ApiKey \"{ApiKey}\"");
        }
    }
}
