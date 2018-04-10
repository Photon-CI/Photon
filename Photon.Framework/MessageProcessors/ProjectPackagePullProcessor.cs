using Photon.Communication;
using Photon.Communication.Messages;
using Photon.Framework.Scripts;
using Photon.Framework.TcpMessages;
using System;
using System.Threading.Tasks;

namespace Photon.Framework.MessageProcessors
{
    internal class ProjectPackagePullProcessor : MessageProcessorBase<ProjectPackagePullRequest>
    {
        public override async Task<IResponseMessage> Process(ProjectPackagePullRequest requestMessage)
        {
            if (!(Transceiver.Context is IServerContext sessionContext)) throw new Exception("Server Context is undefined!");

            var response = new ProjectPackagePullResponse();

            if (sessionContext.ProjectPackages.TryGet(requestMessage.ProjectPackageId, requestMessage.ProjectPackageVersion, out var packageFilename)) {
                response.Successful = true;
                response.Filename = packageFilename;
            }
            else {
                response.Exception = $"Project Package '{requestMessage.ProjectPackageId}.{requestMessage.ProjectPackageVersion}' not found!";
            }

            return await Task.FromResult(response);
        }
    }
}
