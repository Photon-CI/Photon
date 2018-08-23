using Photon.Framework.Domain;
using Photon.Framework.Extensions;
using Photon.Framework.Packages;
using Photon.Library.Packages;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Photon.Server.Internal.Packages
{
    internal class PackageHost : IDisposable
    {
        private readonly ProjectPackageManager projectPackages;
        private readonly ApplicationPackageManager applicationPackages;
        private readonly List<PackageReference> PushedProjectPackageList;
        private readonly List<PackageReference> PushedApplicationPackageList;

        public IEnumerable<PackageReference> PushedProjectPackages => PushedProjectPackageList;
        public IEnumerable<PackageReference> PushedApplicationPackages => PushedApplicationPackageList;

        public DomainPackageBoundary Client {get;}


        public PackageHost()
        {
            projectPackages = new ProjectPackageManager {
                PackageDirectory = Configuration.ProjectPackageDirectory,
            };

            applicationPackages = new ApplicationPackageManager {
                PackageDirectory = Configuration.ApplicationPackageDirectory,
            };

            PushedProjectPackageList = new List<PackageReference>();
            PushedApplicationPackageList = new List<PackageReference>();

            Client = new DomainPackageBoundary();
            Client.OnPushProjectPackage += PackageClient_OnPushProjectPackage;
            Client.OnPushApplicationPackage += PackageClient_OnPushApplicationPackage;
            Client.OnPullProjectPackage += PackageClient_OnPullProjectPackage;
            Client.OnPullApplicationPackage += PackageClient_OnPullApplicationPackage;
        }

        public void Dispose()
        {
            Client.Dispose();
        }

        private void PackageClient_OnPushProjectPackage(string filename, RemoteTaskCompletionSource taskHandle)
        {
            Task.Run(async () => {
                var metadata = await ProjectPackageTools.GetMetadataAsync(filename);
                if (metadata == null) throw new ApplicationException($"Invalid Project Package '{filename}'! No metadata found.");

                await projectPackages.Add(filename);
                PushedProjectPackageList.Add(new PackageReference(metadata.Id, metadata.Version));
            }).ContinueWith(taskHandle.FromTask);
        }

        private void PackageClient_OnPushApplicationPackage(string filename, RemoteTaskCompletionSource taskHandle)
        {
            Task.Run(async () => {
                var metadata = await ApplicationPackageTools.GetMetadataAsync(filename);
                if (metadata == null) throw new ApplicationException($"Invalid Project Package '{filename}'! No metadata found.");

                await applicationPackages.Add(filename);
                PushedApplicationPackageList.Add(new PackageReference(metadata.Id, metadata.Version));
            }).ContinueWith(taskHandle.FromTask);
        }

        private void PackageClient_OnPullProjectPackage(string id, string version, RemoteTaskCompletionSource<string> taskHandle)
        {
            Task.Run(async () => {
                if (!projectPackages.TryGet(id, version, out var packageFilename))
                    throw new ApplicationException($"Project Package '{id}.{version}' not found!");

                return await Task.FromResult(packageFilename);
            }).ContinueWith(taskHandle.FromTask);
        }

        private void PackageClient_OnPullApplicationPackage(string id, string version, RemoteTaskCompletionSource<string> taskHandle)
        {
            Task.Run(async () => {
                if (!applicationPackages.TryGet(id, version, out var packageFilename))
                    throw new ApplicationException($"Application Package '{id}.{version}' not found!");

                return await Task.FromResult(packageFilename);
            }).ContinueWith(taskHandle.FromTask);
        }
    }
}
