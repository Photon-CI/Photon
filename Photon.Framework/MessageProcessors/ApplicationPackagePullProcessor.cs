using Photon.Communication;
using Photon.Communication.Messages;
using Photon.Framework.Server;
using Photon.Framework.TcpMessages;
using System;
using System.Threading.Tasks;

namespace Photon.Framework.MessageProcessors
{
    internal class ApplicationPackagePullProcessor : MessageProcessorBase<ApplicationPackagePullRequest>
    {
        public override async Task<IResponseMessage> Process(ApplicationPackagePullRequest requestMessage)
        {
            if (!(Transceiver.Context is IServerContext sessionContext))
                throw new Exception("Server Context is undefined!");

            var response = new ApplicationPackagePullResponse();

            if (!sessionContext.ApplicationPackages.TryGet(requestMessage.PackageId, requestMessage.PackageVersion, out var packageFilename))
                throw new Exception($"Project Package '{requestMessage.PackageId}.{requestMessage.PackageVersion}' not found!");

            response.Filename = packageFilename;

            return await Task.FromResult(response);
        }
    }
}
