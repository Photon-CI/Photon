using NuGet.Configuration;
using NuGet.Packaging;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using Photon.Framework.Domain;
using Photon.Framework.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.NuGetPlugin
{
    public class NuGetTools
    {
        private readonly IDomainContext context;
        private readonly SourceRepository sourceRepository;


        public NuGetTools(IDomainContext context)
        {
            this.context = context;

            var providers = new List<Lazy<INuGetResourceProvider>>();
            providers.AddRange(Repository.Provider.GetCoreV3());  // Add v3 API support
            //providers.AddRange(Repository.Provider.GetCoreV2());  // Add v2 API support
            var packageSource = new PackageSource("https://api.nuget.org/v3/index.json");
            sourceRepository = new SourceRepository(packageSource, providers);
        }

        public async Task<Version[]> GetAllVersions(string packageId, CancellationToken token)
        {
            var filter = new SearchFilter(true);

            var searchResource = await sourceRepository.GetResourceAsync<PackageSearchResource>(token);
            var searchMetadata = await searchResource.SearchAsync(packageId, filter, 0, 100, null, token);

            var resultList = new List<Version>();

            foreach (var result in searchMetadata) {
                if (!string.Equals(result.Identity.Id, packageId, StringComparison.OrdinalIgnoreCase))
                    continue;

                var versionList = (await result.GetVersionsAsync())
                    .Select(resultVersion => resultVersion.Version.Version);

                resultList.AddRange(versionList);
            }

            return resultList.ToArray();
        }

        public void Pack(string nuspecFilename, string packageFilename)
        {
            Manifest nuspec;
            try {
                using (var nuspecStream = File.Open(nuspecFilename, FileMode.Open, FileAccess.Read)) {
                    nuspec = Manifest.ReadFrom(nuspecStream, true);
                }
            }
            catch (FileNotFoundException) {
                context.Output.AppendLine($"Package definition '{nuspecFilename}' not found!", ConsoleColor.DarkYellow);
                throw;
            }
            catch (Exception error) {
                context.Output.AppendLine($"Failed to load package definition '{nuspecFilename}'! {error.UnfoldMessages()}", ConsoleColor.DarkRed);
                throw;
            }

            context.Output.AppendLine($"Creating package '{packageFilename}'...", ConsoleColor.DarkCyan);

            try {
                var builder = new PackageBuilder();
                builder.Populate(nuspec.Metadata);

                using (var packageStream = File.Open(packageFilename, FileMode.Create, FileAccess.Write)) {
                    builder.Save(packageStream);
                }

                context.Output.AppendLine($"Package '{packageFilename}' created successfully.", ConsoleColor.DarkGreen);
            }
            catch (Exception error) {
                context.Output.AppendLine($"Failed to create package '{packageFilename}'! {error.UnfoldMessages()}", ConsoleColor.DarkRed);
                throw;
            }
        }

        public async Task PushAsync(string packageFilename, Func<string, string> apiKeyFunc, CancellationToken token)
        {
            context.Output.AppendLine($"Pushing package '{packageFilename}'...", ConsoleColor.DarkCyan);

            try {
                var updateResource = await sourceRepository.GetResourceAsync<PackageUpdateResource>(token);
                await updateResource.Push(packageFilename, null, 60, false, apiKeyFunc, null, null);

                context.Output.AppendLine($"Package '{packageFilename}' pushed successfully.", ConsoleColor.DarkGreen);
            }
            catch (Exception error) {
                context.Output.AppendLine($"Failed to push package '{packageFilename}'! {error.UnfoldMessages()}", ConsoleColor.DarkRed);
                throw;
            }
        }
    }
}
