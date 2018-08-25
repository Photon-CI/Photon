using Photon.Framework.Packages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Photon.Server.Internal.Packages
{
    internal class ProjectPackageCache
    {
        private readonly Dictionary<string, Dictionary<string, ProjectPackage>> items;

        public IEnumerable<ProjectPackage> All => items.Values.SelectMany(x => x.Values);


        public ProjectPackageCache()
        {
            items = new Dictionary<string, Dictionary<string, ProjectPackage>>(StringComparer.OrdinalIgnoreCase);
        }

        public async Task Initialize()
        {
            var packageMgr = PhotonServer.Instance.ProjectPackages;
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
            var packageMgr = PhotonServer.Instance.ProjectPackages;

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

    //internal class ProjectPackageCacheItem
    //{
    //    public string ProjectId {get; set;}
    //    public string PackageId {get; set;}
    //    public string PackageName {get; set;}
    //    public string PackageDescription {get; set;}
    //    public string PackageVersion {get; set;}
    //}
}
