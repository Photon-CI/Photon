using Photon.Framework.Packages;
using Photon.Library.Packages;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Photon.Server.Internal.Packages
{
    internal class PackageHost
    {
        private readonly ProjectPackageManager projectPackages;
        private readonly ApplicationPackageManager applicationPackages;
        private readonly List<PackageReference> PushedProjectPackageList;
        private readonly List<PackageReference> PushedApplicationPackageList;

        public IEnumerable<PackageReference> PushedProjectPackages => PushedProjectPackageList;
        public IEnumerable<PackageReference> PushedApplicationPackages => PushedApplicationPackageList;

        public string ProjectId {get; set;}


        public PackageHost()
        {
            PushedProjectPackageList = new List<PackageReference>();
            PushedApplicationPackageList = new List<PackageReference>();

            projectPackages = PhotonServer.Instance.ProjectPackages;
            applicationPackages = PhotonServer.Instance.ApplicationPackages;
        }

        private async Task PackageClient_OnPushProjectPackage(string filename)
        {
            var metadata = await ProjectPackageTools.GetMetadataAsync(filename);
            if (metadata == null) throw new ApplicationException($"Invalid Project Package '{filename}'! No metadata found.");

            await projectPackages.Add(filename);
            PushedProjectPackageList.Add(new PackageReference(metadata.Id, metadata.Version));
        }

        private async Task PackageClient_OnPushApplicationPackage(string filename)
        {
            var metadata = await ApplicationPackageTools.GetMetadataAsync(filename);
            if (metadata == null) throw new ApplicationException($"Invalid Project Package '{filename}'! No metadata found.");

            await applicationPackages.Add(filename);
            PushedApplicationPackageList.Add(new PackageReference(metadata.Id, metadata.Version));
        }

        private async Task<string> PackageClient_OnPullProjectPackage(string id, string version)
        {
            if (!projectPackages.TryGet(id, version, out var packageFilename))
                throw new ApplicationException($"Project Package '{id}.{version}' not found!");

            return await Task.FromResult(packageFilename);
        }

        private async Task<string> PackageClient_OnPullApplicationPackage(string id, string version)
        {
            if (!applicationPackages.TryGet(id, version, out var packageFilename))
                throw new ApplicationException($"Application Package '{id}.{version}' not found!");

            return await Task.FromResult(packageFilename);
        }
    }
}
