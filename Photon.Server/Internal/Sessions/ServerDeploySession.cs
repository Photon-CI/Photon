using Photon.Framework.Domain;
using Photon.Framework.Extensions;
using Photon.Framework.Packages;
using Photon.Framework.Projects;
using Photon.Framework.Server;
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
        public Project Project {get; set;}
        public string AssemblyFilename {get; set;}
        public string ScriptName {get; set;}
        public string ProjectPackageId {get; set;}
        public string ProjectPackageVersion {get; set;}
        public string ProjectPackageFilename {get; set;}
        public string EnvironmentName {get; set;}


        protected override DomainAgentSessionHostBase OnCreateHost(ServerAgent agent)
        {
            return new DomainAgentDeploySessionHost(this, agent, TokenSource.Token);
        }

        public override async Task PrepareWorkDirectoryAsync()
        {
            await base.PrepareWorkDirectoryAsync();

            var metadata = await ProjectPackageTools.UnpackAsync(ProjectPackageFilename, BinDirectory);

            AssemblyFilename = metadata.AssemblyFilename;
            ScriptName = metadata.ScriptName;
        }

        public override async Task RunAsync()
        {
            var assemblyFilename = Path.Combine(BinDirectory, AssemblyFilename);
            if (!File.Exists(assemblyFilename))
                throw new FileNotFoundException($"The assembly file '{assemblyFilename}' could not be found!");

            Domain = new ServerDomain();
            Domain.Initialize(assemblyFilename);

            using (var contextOutput = new DomainOutput()) {
                contextOutput.OnWrite += (text, color) => Output.Write(text, color);
                contextOutput.OnWriteLine += (text, color) => Output.WriteLine(text, color);
                contextOutput.OnWriteRaw += (text) => Output.WriteRaw(text);

                var agents = PhotonServer.Instance.Agents.All.ToArray();

                if (!string.IsNullOrEmpty(EnvironmentName)) {
                    var env = Project.Environments
                        .FirstOrDefault(x => string.Equals(x.Name, EnvironmentName));

                    if (env == null) throw new ApplicationException($"Environment '{EnvironmentName}' not found!");

                    agents = agents.Where(x => env.AgentIdList.Contains(x.Id, StringComparer.OrdinalIgnoreCase)).ToArray();
                }

                var packageClient = new DomainPackageClient(Packages.Client);

                var context = new ServerDeployContext {
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
                    Packages = packageClient,
                    ConnectionFactory = ConnectionFactory,
                    Output = contextOutput,
                    ServerVariables = Variables,
                };

                try {
                    await Domain.RunDeployScript(context, TokenSource.Token);

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
                }
            }
        }
    }
}
