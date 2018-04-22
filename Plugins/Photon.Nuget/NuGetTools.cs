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
using NuGet.Common;

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

        public async Task<string[]> GetAllVersions(string packageId, CancellationToken token)
        {
            var cache = new SourceCacheContext {
                DirectDownload = true,
                NoCache = true,
            };

            var searchResource = await sourceRepository.GetResourceAsync<FindPackageByIdResource>(token);

            if (searchResource == null) throw new ApplicationException("Unable to retrieve package locator resource!");

            var logger = new NullLogger();
            var versionList = (await searchResource.GetAllVersionsAsync(packageId, cache, logger, token))?.ToArray();

            if (versionList == null) throw new ApplicationException("Unable to retrieve version list!");

            return versionList.Select(x => x.ToString()).ToArray();
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
