using Photon.Communication;
using Photon.Communication.Messages;
using Photon.Server.Internal;
using System;
using System.Threading.Tasks;
using Photon.Library.TcpMessages;

namespace Photon.Server.MessageProcessors
{
    internal class ApplicationPackagePullProcessor : MessageProcessorBase<ApplicationPackagePullRequest>
    {
        public override async Task<IResponseMessage> Process(ApplicationPackagePullRequest requestMessage)
        {
            //if (!(Transceiver.Context is IServerSession session))
            //    throw new Exception("Session is undefined!");

            if (!PhotonServer.Instance.ApplicationPackages.TryGet(requestMessage.PackageId, requestMessage.PackageVersion, out var packageFilename))
                throw new Exception($"Project Package '{requestMessage.PackageId}.{requestMessage.PackageVersion}' not found!");

            var response = new ApplicationPackagePullResponse {
                Filename = packageFilename,
            };

            return await Task.FromResult(response);
        }
    }
}
