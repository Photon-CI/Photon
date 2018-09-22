using Photon.Communication;
using Photon.Framework.Domain;
using Photon.Framework.Extensions;
using Photon.Framework.Packages;
using Photon.Library.TcpMessages;
using System;
using System.Threading.Tasks;
using Photon.Library.TcpMessages.Packages;

namespace Photon.Agent.Internal.Packages
{
    internal class PackageHost : IDisposable
    {
        public DomainPackageBoundary Boundary {get;}
        public string ServerSessionId {get; set;}
        public MessageTransceiver Transceiver {get; set;}


        public PackageHost()
        {
            Boundary = new DomainPackageBoundary();
            Boundary.OnPushProjectPackage += PackageClient_OnPushProjectPackage;
            Boundary.OnPushApplicationPackage += PackageClient_OnPushApplicationPackage;
            Boundary.OnPullProjectPackage += PackageClient_OnPullProjectPackage;
            Boundary.OnPullApplicationPackage += PackageClient_OnPullApplicationPackage;
        }

        public void Dispose()
        {
            Boundary.Dispose();
        }

        private void PackageClient_OnPushProjectPackage(string filename, RemoteTaskCompletionSource taskHandle)
        {
            var packageRequest = new AgentProjectPackagePushRequest {
                ServerSessionId = ServerSessionId,
                Filename = filename,
            };

            Transceiver.Send(packageRequest)
                .GetResponseAsync()
                .ContinueWith(taskHandle.FromTask);
        }

        private void PackageClient_OnPushApplicationPackage(string filename, RemoteTaskCompletionSource taskHandle)
        {
            var packageRequest = new ApplicationPackagePushRequest {
                ServerSessionId = ServerSessionId,
                Filename = filename,
            };

            Transceiver.Send(packageRequest)
                .GetResponseAsync()
                .ContinueWith(taskHandle.FromTask);
        }

        private void PackageClient_OnPullProjectPackage(string id, string version, RemoteTaskCompletionSource<string> taskHandle)
        {
            var packageRequest = new AgentProjectPackagePullRequest {
                ProjectPackageId = id,
                ProjectPackageVersion = version,
            };

            Task.Run(async () => {
                var response = await Transceiver.Send(packageRequest)
                    .GetResponseAsync<AgentProjectPackagePullResponse>();

                return response.Filename;
            }).ContinueWith(taskHandle.FromTask);
        }

        private void PackageClient_OnPullApplicationPackage(string id, string version, RemoteTaskCompletionSource<string> taskHandle)
        {
            Task.Run(async () => {
                var packageRequest = new ApplicationPackagePullRequest {
                    PackageId = id,
                    PackageVersion = version,
                };

                var response = await Transceiver.Send(packageRequest)
                    .GetResponseAsync<ApplicationPackagePullResponse>();

                return response.Filename;
            }).ContinueWith(taskHandle.FromTask);
        }
    }
}
