using Photon.Communication;
using Photon.Communication.Messages;
using Photon.Framework.Server;
using Photon.Framework.TcpMessages;
using System;
using System.Threading.Tasks;

namespace Photon.Framework.MessageProcessors
{
    internal class ProjectPackagePullProcessor : MessageProcessorBase<ProjectPackagePullRequest>
    {
        public override async Task<IResponseMessage> Process(ProjectPackagePullRequest requestMessage)
        {
            if (!(Transceiver.Context is IServerContext sessionContext))
                throw new Exception("Server Context is undefined!");

            var response = new ProjectPackagePullResponse();

            if (!sessionContext.ProjectPackages.TryGet(requestMessage.ProjectPackageId, requestMessage.ProjectPackageVersion, out var packageFilename))
                throw new Exception($"Project Package '{requestMessage.ProjectPackageId}.{requestMessage.ProjectPackageVersion}' not found!");

            response.Filename = packageFilename;

            return await Task.FromResult(response);
        }
    }
}
