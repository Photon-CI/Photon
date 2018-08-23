using Photon.Framework.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Framework.Packages
{
    [Serializable]
    public class DomainPackageClient
    {
        private readonly DomainPackageBoundary packageMgr;


        public DomainPackageClient(DomainPackageBoundary packageMgr)
        {
            this.packageMgr = packageMgr;
        }

        public async Task PushProjectPackageAsync(string filename, CancellationToken token = default(CancellationToken))
        {
            await RemoteTaskCompletionSource.Run(task => {
                packageMgr.PushProjectPackage(filename, task);
            }, token);
        }

        public async Task PushApplicationPackageAsync(string filename, CancellationToken token = default(CancellationToken))
        {
            await RemoteTaskCompletionSource.Run(task => {
                packageMgr.PushApplicationPackage(filename, task);
            }, token);
        }

        public async Task<string> PullProjectPackageAsync(string id, string version)
        {
            return await RemoteTaskCompletionSource<string>.Run(task => {
                packageMgr.PullProjectPackage(id, version, task);
            });
        }

        public async Task<string> PullApplicationPackageAsync(string id, string version)
        {
            return await RemoteTaskCompletionSource<string>.Run(task => {
                packageMgr.PullApplicationPackage(id, version, task);
            });
        }
    }
}
