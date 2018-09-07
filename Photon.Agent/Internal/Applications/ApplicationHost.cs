using Photon.Framework.Applications;
using Photon.Framework.Domain;
using System;
using System.IO;

namespace Photon.Agent.Internal.Applications
{
    public class ApplicationHost : IDisposable
    {
        public ApplicationManagerBoundary Boundary {get;}


        public ApplicationHost()
        {
            Boundary = new ApplicationManagerBoundary();
            Boundary.OnGetApplicationRevision += AppMgr_OnGetApplicationRevision;
            Boundary.OnRegisterApplicationRevision += AppMgr_OnRegisterApplicationRevision;
        }

        public void Dispose()
        {
            Boundary.Dispose();
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

            var _rev = GetDomainRevision(app, revision);

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
                EnvironmentName = appRevisionRequest.EnvironmentName,
                PackageId = appRevisionRequest.PackageId,
                PackageVersion = appRevisionRequest.PackageVersion,
                Location = Path.Combine(app.Location, pathName),
                Time = DateTime.Now,
            };

            app.RegisterRevision(revision);
            appMgr.Save();

            revision.Initialize();

            var _rev = GetDomainRevision(app, revision);

            taskHandle.SetResult(_rev);
        }

        private DomainApplicationRevision GetDomainRevision(Application app, ApplicationRevision revision)
        {
            return new DomainApplicationRevision {
                ProjectId = app.ProjectId,
                ApplicationName = app.Name,
                ApplicationPath = revision.Location,
                DeploymentNumber = revision.DeploymentNumber,
                EnvironmentName = revision.EnvironmentName,
                PackageId = revision.PackageId,
                PackageVersion = revision.PackageVersion,
                CreatedTime = revision.Time,
            };
        }
    }
}
