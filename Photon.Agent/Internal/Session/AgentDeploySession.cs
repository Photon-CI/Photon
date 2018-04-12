using Photon.Communication;
using Photon.Framework.Agent;
using Photon.Framework.Packages;
using Photon.Framework.Tasks;
using Photon.Framework.TcpMessages;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Photon.Agent.Internal.Session
{
    internal class AgentDeploySession : AgentSessionBase
    {
        public ProjectPackage Metadata {get; private set;}
        public string ProjectPackageId {get; set;}
        public string ProjectPackageVersion {get; set;}
        public string ApplicationsDirectory {get; set;}


        public AgentDeploySession(MessageTransceiver transceiver, string serverSessionId) : base(transceiver, serverSessionId)
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

        public override async Task<TaskResult> RunTaskAsync(string taskName, string taskSessionId)
        {
            var context = new AgentDeployContext {
                Project = Project,
                ProjectPackageId = ProjectPackageId,
                ProjectPackageVersion = ProjectPackageVersion,
                AssemblyFilename = AssemblyFilename,
                TaskName = taskName,
                WorkDirectory = WorkDirectory,
                ContentDirectory = ContentDirectory,
                BinDirectory = BinDirectory,
                ApplicationsDirectory = ApplicationsDirectory,
                Output = Output.Writer,
                Packages = PackageClient,
            };

            return await Domain.RunDeployTask(context);
        }
    }
}
