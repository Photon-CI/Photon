using Photon.Framework.Packages;
using Photon.Library.Packages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Photon.Server.Internal.Packages
{
    internal class ProjectPackageCache
    {
        private readonly ProjectPackageManager packageMgr;
        private readonly Dictionary<string, Dictionary<string, ProjectPackage>> items;

        public IEnumerable<ProjectPackage> All => items.Values.SelectMany(x => x.Values);


        public ProjectPackageCache(ProjectPackageManager packageMgr)
        {
            this.packageMgr = packageMgr;

            items = new Dictionary<string, Dictionary<string, ProjectPackage>>(StringComparer.OrdinalIgnoreCase);
        }

        public async Task Initialize()
        {
            var packageIdList = packageMgr.GetAllPackages().ToArray();

            foreach (var packageId in packageIdList) {
                if (!items.TryGetValue(packageId, out var cacheVersionList)) {
                    cacheVersionList = new Dictionary<string, ProjectPackage>(StringComparer.OrdinalIgnoreCase);
                    items[packageId] = cacheVersionList;
                }

                var versionList = packageMgr.GetAllPackageVersions(packageId).ToArray();

                foreach (var version in versionList) {
                    if (cacheVersionList.ContainsKey(version)) continue;

                    cacheVersionList[version] = await LoadItem(packageId, version);
                }
            }
        }

        private async Task<ProjectPackage> LoadItem(string packageId, string version)
        {
            if (!packageMgr.TryGet(packageId, version, out var filename))
                throw new ApplicationException($"Package '{packageId}' version '{version}' not found!");

            return await ProjectPackageTools.GetMetadataAsync(filename);
        }

        public void AddPackage(ProjectPackage item)
        {
            if (!items.TryGetValue(item.Id, out var cacheVersionList)) {
                cacheVersionList = new Dictionary<string, ProjectPackage>(StringComparer.OrdinalIgnoreCase);
                items[item.Id] = cacheVersionList;
            }

            cacheVersionList[item.Version] = item;
        }
    }
}
