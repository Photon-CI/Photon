using Photon.Communication;
using Photon.Framework.Agent;
using Photon.Framework.Applications;
using Photon.Framework.Domain;
using Photon.Framework.Packages;
using Photon.Library.TcpMessages;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Photon.Agent.Internal.Session
{
    internal class AgentDeploySession : AgentSessionBase
    {
        public uint DeploymentNumber {get; set;}
        public ProjectPackage Metadata {get; private set;}
        public string ProjectPackageId {get; set;}
        public string ProjectPackageVersion {get; set;}
        public string ApplicationsDirectory {get; set;}
        public string EnvironmentName {get; set;}


        public AgentDeploySession(MessageTransceiver transceiver, string serverSessionId, string sessionClientId) : base(transceiver, serverSessionId, sessionClientId)
        {
            ApplicationsDirectory = Configuration.ApplicationsDirectory;
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            await DownloadProjectPackage();

            LoadProjectAssembly();
        }

        private async Task DownloadProjectPackage()
        {
            var packageRequest = new ProjectPackagePullRequest {
                ProjectPackageId = ProjectPackageId,
                ProjectPackageVersion = ProjectPackageVersion,
            };

            ProjectPackagePullResponse packageResponse;
            try {
                packageResponse = await Transceiver.Send(packageRequest)
                    .GetResponseAsync<ProjectPackagePullResponse>();
            }
            catch (Exception error) {
                throw new ApplicationException($"Failed to download package '{ProjectPackageId}.{ProjectPackageVersion}'! {error.Message}");
            }

            try {
                Metadata = await ProjectPackageTools.UnpackAsync(packageResponse.Filename, BinDirectory);

                AssemblyFilename = Path.Combine(BinDirectory, Metadata.AssemblyFilename);
            }
            finally {
                try {
                    File.Delete(packageResponse.Filename);
                }
                catch (Exception error) {
                    Log.Warn("Failed to remove temporary package file!", error);
                }
            }
        }

        private void LoadProjectAssembly()
        {
            if (!File.Exists(AssemblyFilename)) {
                Output.WriteLine($"The assembly file '{AssemblyFilename}' could not be found!", ConsoleColor.DarkYellow);
                throw new ApplicationException($"The assembly file '{AssemblyFilename}' could not be found!");
            }

            try {
                Domain = new AgentSessionDomain();
                Domain.Initialize(AssemblyFilename);
            }
            catch (Exception error) {
                Output.WriteLine($"An error occurred while initializing the assembly! {error.Message} [{SessionId}]", ConsoleColor.DarkRed);
                throw new ApplicationException($"Failed to initialize assembly! [{SessionId}]", error);
            }
        }

        public override async Task RunTaskAsync(string taskName, string taskSessionId)
        {
            if (taskName == null) throw new ArgumentNullException(nameof(taskName));
            if (taskSessionId == null) throw new ArgumentNullException(nameof(taskSessionId));

            var domainOutput = new DomainOutput();
            domainOutput.OnWrite += (text, color) => Output.Write(text, color);
            domainOutput.OnWriteLine += (text, color) => Output.WriteLine(text, color);
            domainOutput.OnWriteRaw += text => Output.WriteRaw(text);

            var applicationClient = new ApplicationManagerClient(Applications.Boundary);
            var packageClient = new DomainPackageClient(Packages.Boundary);

            var context = new AgentDeployContext {
                DeploymentNumber = DeploymentNumber,
                Project = Project,
                ProjectPackageId = ProjectPackageId,
                ProjectPackageVersion = ProjectPackageVersion,
                AssemblyFilename = AssemblyFilename,
                TaskName = taskName,
                WorkDirectory = WorkDirectory,
                ContentDirectory = ContentDirectory,
                BinDirectory = BinDirectory,
                ApplicationsDirectory = ApplicationsDirectory,
                Output = domainOutput,
                Packages = packageClient,
                AgentVariables = AgentVariables,
                ServerVariables = ServerVariables,
                Applications = applicationClient,
                EnvironmentName = EnvironmentName,
                Agent = Agent,
            };

            try {
                await Domain.RunDeployTask(context, TokenSource.Token);
            }
            catch (Exception error) {
                Exception = error;
                throw;
            }
        }

        public override async Task CompleteAsync()
        {
            // TODO ?
        }

        //private void AppMgr_OnGetApplicationRevision(string projectId, string appName, uint deploymentNumber, RemoteTaskCompletionSource<DomainApplicationRevision> taskHandle)
        //{
        //    var app = PhotonAgent.Instance.ApplicationMgr.GetApplication(projectId, appName);
        //    if (app == null) {
        //        taskHandle.SetResult(null);
        //        return;
        //    }

        //    var revision = app.GetRevision(deploymentNumber);
        //    if (revision == null) {
        //        taskHandle.SetResult(null);
        //        return;
        //    }

        //    var _rev = new DomainApplicationRevision {
        //        ProjectId = app.ProjectId,
        //        ApplicationName = app.Name,
        //        ApplicationPath = revision.Location,
        //        DeploymentNumber = revision.DeploymentNumber,
        //        PackageId = revision.PackageId,
        //        PackageVersion = revision.PackageVersion,
        //        CreatedTime = revision.Time,
        //    };

        //    taskHandle.SetResult(_rev);
        //}

        //private void AppMgr_OnRegisterApplicationRevision(DomainApplicationRevisionRequest appRevisionRequest, RemoteTaskCompletionSource<DomainApplicationRevision> taskHandle)
        //{
        //    var appMgr = PhotonAgent.Instance.ApplicationMgr;
        //    var app = appMgr.GetApplication(appRevisionRequest.ProjectId, appRevisionRequest.ApplicationName)
        //        ?? appMgr.RegisterApplication(appRevisionRequest.ProjectId, appRevisionRequest.ApplicationName);

        //    var pathName = appRevisionRequest.DeploymentNumber.ToString();

        //    var revision = new ApplicationRevision {
        //        DeploymentNumber = appRevisionRequest.DeploymentNumber,
        //        PackageId = appRevisionRequest.PackageId,
        //        PackageVersion = appRevisionRequest.PackageVersion,
        //        Location = NetPath.Combine(app.Location, pathName),
        //        Time = DateTime.Now,
        //    };

        //    app.Revisions.Add(revision);
        //    appMgr.Save();

        //    revision.Initialize();

        //    var _rev = new DomainApplicationRevision {
        //        ProjectId = app.ProjectId,
        //        ApplicationName = app.Name,
        //        ApplicationPath = revision.Location,
        //        DeploymentNumber = revision.DeploymentNumber,
        //        PackageId = revision.PackageId,
        //        PackageVersion = revision.PackageVersion,
        //        CreatedTime = revision.Time,
        //    };

        //    taskHandle.SetResult(_rev);
        //}
    }
}
