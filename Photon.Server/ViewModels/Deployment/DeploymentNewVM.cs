using Photon.Server.Internal;
using Photon.Server.Internal.Sessions;
using PiServerLite.Http.Handlers;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace Photon.Server.ViewModels.Deployment
{
    internal class DeploymentNewVM : ServerViewModel
    {
        public string ProjectId {get; set;}
        public string PackageId {get; set;}
        public string PackageVersion {get; set;}
        public string EnvironmentName {get; set;}
        public string ProjectName {get; set;}
        public string ProjectDescription {get; set;}
        public uint DeploymentNumber {get; private set;}
        public string ServerSessionId {get; private set;}

        public DeploymentEnvironmentRow[] Environments {get; set;}


        public DeploymentNewVM(IHttpHandler handler) : base(handler)
        {
            PageTitle = "Photon Server New Deployment";
        }

        protected override void OnBuild()
        {
            base.OnBuild();

            if (!PhotonServer.Instance.Projects.TryGet(ProjectId, out var project))
                throw new ApplicationException($"Project '{ProjectId}' not found!");

            ProjectName = project.Description.Name;
            ProjectDescription = project.Description.Description;

            Environments = project.Description.Environments
                .Select(x => new DeploymentEnvironmentRow {
                    Name = x.Name,
                    Selected = string.Equals(x.Name, EnvironmentName, StringComparison.OrdinalIgnoreCase),
                }).ToArray();
        }

        public void Restore(NameValueCollection form)
        {
            ProjectId = form[nameof(ProjectId)];
            PackageId = form[nameof(PackageId)];
            PackageVersion = form[nameof(PackageVersion)];
            EnvironmentName = form[nameof(EnvironmentName)];
        }

        public async Task StartDeployment()
        {
            if (!PhotonServer.Instance.ProjectPackages.TryGet(PackageId, PackageVersion, out var packageFilename))
                throw new ApplicationException($"Project Package '{PackageId}.{PackageVersion}' was not found!");

            if (string.IsNullOrEmpty(ProjectId))
                throw new ApplicationException("'project' is undefined!");

            if (!PhotonServer.Instance.Projects.TryGet(ProjectId, out var project))
                throw new ApplicationException($"Project '{ProjectId}' was not found!");

            var deployment = await project.StartNewDeployment();
            deployment.PackageId = PackageId;
            deployment.PackageVersion = PackageVersion;
            deployment.EnvironmentName = EnvironmentName;

            var session = new ServerDeploySession {
                Project = project.Description,
                Deployment = deployment,
                ProjectPackageId = PackageId,
                ProjectPackageVersion = PackageVersion,
                ProjectPackageFilename = packageFilename,
                EnvironmentName = EnvironmentName,
            };

            deployment.ServerSessionId = session.SessionId;

            PhotonServer.Instance.Sessions.BeginSession(session);
            PhotonServer.Instance.Queue.Add(session);

            ServerSessionId = session.SessionId;
            DeploymentNumber = session.Deployment.Number;
        }
    }

    internal class DeploymentEnvironmentRow
    {
        public string Name {get; set;}
        public bool Selected {get; set;}
    }
}
