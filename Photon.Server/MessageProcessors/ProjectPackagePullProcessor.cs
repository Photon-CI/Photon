using Photon.Communication;
using Photon.Communication.Messages;
using Photon.Library.TcpMessages.Packages;
using Photon.Server.Internal;
using System;
using System.Threading.Tasks;

namespace Photon.Server.MessageProcessors
{
    internal class ProjectPackagePullProcessor : MessageProcessorBase<AgentProjectPackagePullRequest>
    {
        public override Task<IResponseMessage> Process(AgentProjectPackagePullRequest requestMessage)
        {
            if (!PhotonServer.Instance.ProjectPackages.TryGet(requestMessage.ProjectPackageId, requestMessage.ProjectPackageVersion, out var packageFilename))
                throw new Exception($"Project Package '{requestMessage.ProjectPackageId}.{requestMessage.ProjectPackageVersion}' not found!");

            return Task.FromResult<IResponseMessage>(new AgentProjectPackagePullResponse {
                Filename = packageFilename,
            });
        }
    }
}
