using Photon.Server.Internal;
using Photon.Server.Internal.Sessions;
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
        //public string ScriptName {get; set;}
        public uint DeploymentNumber {get; private set;}
        public string ServerSessionId {get; private set;}

        public DeploymentEnvironmentRow[] Environments {get; set;}


        public DeploymentNewVM()
        {
            PageTitle = "Photon Server New Deployment";
        }

        public void Build()
        {
            if (!PhotonServer.Instance.Projects.TryGet(ProjectId, out var project))
                throw new ApplicationException($"Project '{ProjectId}' not found!");

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
            //ScriptName = form[nameof(ScriptName)];
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
            deployment.EnvironmentName = EnvironmentName;
            //deployment.ScriptName = ?;

            var session = new ServerDeploySession {
                Project = project.Description,
                Deployment = deployment,
                ProjectPackageId = PackageId,
                ProjectPackageVersion = PackageVersion,
                ProjectPackageFilename = packageFilename,
                //ScriptName = ScriptName,
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
