using Photon.Framework.Domain;
using System;
using System.Threading.Tasks;

namespace Photon.Framework.Applications
{
    [Serializable]
    public class ApplicationManagerClient
    {
        private readonly ApplicationManagerBoundary appMgr;
        public string CurrentProjectId {get; set;}
        public uint CurrentDeploymentNumber {get; set;}


        public ApplicationManagerClient(ApplicationManagerBoundary appMgr)
        {
            this.appMgr = appMgr;
        }

        public async Task<DomainApplicationRevision> GetApplicationRevision(string appName)
        {
            return await GetApplicationRevision(CurrentProjectId, appName, CurrentDeploymentNumber);
        }

        public async Task<DomainApplicationRevision> GetApplicationRevision(string projectId, string appName, uint deploymentNumber)
        {
            return await RemoteTaskCompletionSource<DomainApplicationRevision>.Run(task => {
                appMgr.GetApplicationRevision(projectId, appName, deploymentNumber, task);
            });
        }

        public async Task<DomainApplicationRevision> RegisterApplicationRevision(string appName, string packageId, string packageVersion, string environmentName = null)
        {
            var revisionRequest = new DomainApplicationRevisionRequest {
                ProjectId = CurrentProjectId,
                ApplicationName = appName,
                DeploymentNumber = CurrentDeploymentNumber,
                PackageId = packageId,
                PackageVersion = packageVersion,
                EnvironmentName = environmentName,
            };

            return await RegisterApplicationRevision(revisionRequest);
        }

        public async Task<DomainApplicationRevision> RegisterApplicationRevision(DomainApplicationRevisionRequest revisionRequest)
        {
            return await RemoteTaskCompletionSource<DomainApplicationRevision>.Run(task => {
                appMgr.RegisterApplicationRevision(revisionRequest, task);
            });
        }
    }
}
