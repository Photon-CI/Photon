using Photon.Framework.Domain;
using System;
using System.Threading.Tasks;

namespace Photon.Framework.Applications
{
    [Serializable]
    public class ApplicationManagerClient
    {
        private readonly ApplicationManagerBoundary appMgr;


        public ApplicationManagerClient(ApplicationManagerBoundary appMgr)
        {
            this.appMgr = appMgr;
        }

        public async Task<DomainApplicationRevision> GetApplicationRevision(string projectId, string appName, uint deploymentNumber)
        {
            return await RemoteTaskCompletionSource<DomainApplicationRevision>.Run(task => {
                appMgr.GetApplicationRevision(projectId, appName, deploymentNumber, task);
            });
        }

        public async Task<DomainApplicationRevision> RegisterApplicationRevision(DomainApplicationRevisionRequest revisionRequest)
        {
            return await RemoteTaskCompletionSource<DomainApplicationRevision>.Run(task => {
                appMgr.RegisterApplicationRevision(revisionRequest, task);
            });
        }
    }
}
