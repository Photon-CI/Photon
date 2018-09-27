using Photon.Communication;
using Photon.Communication.Messages;
using Photon.Library.TcpMessages;
using Photon.Server.Internal;
using System;
using System.Threading.Tasks;

namespace Photon.Server.MessageProcessors
{
    internal class ApplicationPackagePullProcessor : MessageProcessorBase<ApplicationPackagePullRequest>
    {
        public override async Task<IResponseMessage> Process(ApplicationPackagePullRequest requestMessage)
        {
            var serverContext = PhotonServer.Instance.Context;

            if (!serverContext.ApplicationPackages.TryGet(requestMessage.PackageId, requestMessage.PackageVersion, out var packageFilename))
                throw new Exception($"Project Package '{requestMessage.PackageId}.{requestMessage.PackageVersion}' not found!");

            var response = new ApplicationPackagePullResponse {
                Filename = packageFilename,
            };

            return await Task.FromResult(response);
        }
    }
}
