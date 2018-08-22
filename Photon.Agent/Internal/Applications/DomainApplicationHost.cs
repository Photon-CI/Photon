using Photon.Framework;
using Photon.Framework.Applications;
using Photon.Framework.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Agent.Internal.Applications
{
    public class DomainApplicationHost
    {
        public DomainApplicationClient Client {get;}


        public DomainApplicationHost()
        {
            Client = new DomainApplicationClient();
            Client.OnGetApplicationRevision += AppMgr_OnGetApplicationRevision;
            Client.OnRegisterApplicationRevision += AppMgr_OnRegisterApplicationRevision;
        }

        public async Task<DomainApplicationRevision> GetApplicationRevision(string projectId, string appName, uint deploymentNumber, CancellationToken token = default(CancellationToken))
        {
            return await RemoteTaskCompletionSource<DomainApplicationRevision>.Run(task => {
                Client.GetApplicationRevision(projectId, appName, deploymentNumber, task);
            }, token);
        }

        public async Task<DomainApplicationRevision> RegisterApplicationRevision(DomainApplicationRevisionRequest appRevisionRequest, CancellationToken token = default(CancellationToken))
        {
            return await RemoteTaskCompletionSource<DomainApplicationRevision>.Run(task => {
                Client.RegisterApplicationRevision(appRevisionRequest, task);
            }, token);
        }

        private void AppMgr_OnGetApplicationRevision(string projectId, string appName, uint deploymentNumber, RemoteTaskCompletionSource<DomainApplicationRevision> taskHandle)
        {
            var app = PhotonAgent.Instance.ApplicationMgr.GetApplication(projectId, appName);
            if (app == null) {
                taskHandle.SetResult(null);
                return;
            }

            var revision = app.GetRevision(deploymentNumber);
            if (revision == null) {
                taskHandle.SetResult(null);
                return;
            }

            var _rev = new DomainApplicationRevision {
                ProjectId = app.ProjectId,
                ApplicationName = app.Name,
                ApplicationPath = revision.Location,
                DeploymentNumber = revision.DeploymentNumber,
                PackageId = revision.PackageId,
                PackageVersion = revision.PackageVersion,
                CreatedTime = revision.Time,
            };

            taskHandle.SetResult(_rev);
        }

        private void AppMgr_OnRegisterApplicationRevision(DomainApplicationRevisionRequest appRevisionRequest, RemoteTaskCompletionSource<DomainApplicationRevision> taskHandle)
        {
            var appMgr = PhotonAgent.Instance.ApplicationMgr;
            var app = appMgr.GetApplication(appRevisionRequest.ProjectId, appRevisionRequest.ApplicationName)
                ?? appMgr.RegisterApplication(appRevisionRequest.ProjectId, appRevisionRequest.ApplicationName);

            var pathName = appRevisionRequest.DeploymentNumber.ToString();

            var revision = new ApplicationRevision {
                DeploymentNumber = appRevisionRequest.DeploymentNumber,
                PackageId = appRevisionRequest.PackageId,
                PackageVersion = appRevisionRequest.PackageVersion,
                Location = NetPath.Combine(app.Location, pathName),
                Time = DateTime.Now,
            };

            app.Revisions.Add(revision);
            appMgr.Save();

            revision.Initialize();

            var _rev = new DomainApplicationRevision {
                ProjectId = app.ProjectId,
                ApplicationName = app.Name,
                ApplicationPath = revision.Location,
                DeploymentNumber = revision.DeploymentNumber,
                PackageId = revision.PackageId,
                PackageVersion = revision.PackageVersion,
                CreatedTime = revision.Time,
            };

            taskHandle.SetResult(_rev);
        }
    }
}
