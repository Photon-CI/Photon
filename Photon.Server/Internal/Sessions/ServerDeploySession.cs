using Photon.Framework.Extensions;
using Photon.Framework.Packages;
using Photon.Library.Communication.Messages.Session;
using Photon.Server.Internal.Deployments;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Photon.Server.Internal.Sessions
{
    internal class ServerDeploySession : ServerSessionBase
    {
        public DeploymentData Deployment {get; set;}
        public string AssemblyFilename {get; set;}
        public string ScriptName {get; set;}
        public string ProjectPackageId {get; set;}
        public string ProjectPackageVersion {get; set;}
        public string ProjectPackageFilename {get; set;}
        public string EnvironmentName {get; set;}


        public ServerDeploySession(ServerContext context) : base(context) {}

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            var metadata = await ProjectPackageTools.UnpackAsync(ProjectPackageFilename, BinDirectory);

            AssemblyFilename = metadata.AssemblyFilename;
            ScriptName = metadata.ScriptName;
        }

        public override async Task RunAsync()
        {
            var assemblyFilename = Path.Combine(BinDirectory, AssemblyFilename);

            if (!File.Exists(assemblyFilename))
                throw new FileNotFoundException($"The assembly file '{assemblyFilename}' could not be found!");

            //---

            var agents = PhotonServer.Instance.Agents.All.ToArray();

            if (!string.IsNullOrEmpty(EnvironmentName)) {
                var env = Project.Environments
                    .FirstOrDefault(x => string.Equals(x.Name, EnvironmentName));

                if (env == null) throw new ApplicationException($"Environment '{EnvironmentName}' not found!");

                agents = agents.Where(x => env.AgentIdList.Contains(x.Id, StringComparer.OrdinalIgnoreCase)).ToArray();
            }

            //---

            var serverDeployScriptWorker = Context.Workers.Create();
            //...

            try {
                serverDeployScriptWorker.Connect();

                var request = new WorkerServerDeploymentSessionRunRequest {
                    DeploymentNumber = Deployment.Number,
                    Project = Project,
                    Agents = agents,
                    ProjectPackageId = ProjectPackageId,
                    ProjectPackageVersion = ProjectPackageVersion,
                    EnvironmentName = EnvironmentName,
                    ScriptName = ScriptName,
                    WorkDirectory = WorkDirectory,
                    BinDirectory = BinDirectory,
                    ContentDirectory = ContentDirectory,
                    ServerVariables = ServerVariables,
                };

                var response = await serverDeployScriptWorker.Transceiver.Send(request)
                    .GetResponseAsync<WorkerServerDeploymentSessionRunResponse>();

                // TODO

                Deployment.IsSuccess = true;
            }
            catch (OperationCanceledException) {
                Deployment.IsCancelled = true;
                throw;
            }
            catch (Exception error) {
                Deployment.Exception = error.UnfoldMessages();
                throw;
            }
            finally {
                Deployment.IsComplete = true;
                Deployment.Duration = DateTime.UtcNow - Deployment.Created;
                //Deployment.ApplicationPackages = ?;
                Deployment.Save();

                await Deployment.SetOutput(Output.GetString());
                // TODO: Save alternate version with ansi characters removed

                try {
                    serverDeployScriptWorker.Disconnect();
                }
                finally {
                    serverDeployScriptWorker.Dispose();
                }
            }
        }
    }
}
