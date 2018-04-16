using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.NuGetPlugin
{
    public class NuGetTools
    {
        private readonly SourceRepository sourceRepository;


        public NuGetTools()
        {
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

        public async Task PushAsync(string packageFilename, CancellationToken token)
        {
            //var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            //var nugetPath = Path.Combine(appDataPath, "NuGet");
            //var settings = Settings.LoadDefaultSettings(nugetPath);

            var apiKey = (Func<string, string>)(x => "?");
            var symbolApiKey = (Func<string, string>)(x => null);

            var updateResource = await sourceRepository.GetResourceAsync<PackageUpdateResource>(token);
            await updateResource.Push(packageFilename, null, 60, false, apiKey, symbolApiKey, null);
        }
    }
}
