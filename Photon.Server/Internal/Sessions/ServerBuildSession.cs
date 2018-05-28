using Photon.Framework.Domain;
using Photon.Framework.Extensions;
using Photon.Framework.Projects;
using Photon.Framework.Server;
using Photon.Library.GitHub;
using Photon.Library.HttpMessages;
using Photon.Server.Internal.Builds;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Photon.Server.Internal.Sessions
{
    internal class ServerBuildSession : ServerSessionBase
    {
        public BuildData Build {get; set;}
        public Project Project {get; set;}
        public string AssemblyFilename {get; set;}
        public string PreBuild {get; set;}
        public string TaskName {get; set;}
        public string GitRefspec {get; set;}
        public string[] Roles {get; set;}
        public AgentStartModes Mode {get; set;}
        public GithubCommit Commit {get; set;}


        public override async Task RunAsync()
        {
            var contextOutput = new DomainOutput();
            contextOutput.OnWrite += (text, color) => Output.Write(text, color);
            contextOutput.OnWriteLine += (text, color) => Output.WriteLine(text, color);

            var context = new ServerBuildContext {
                BuildNumber = Build.Number,
                Agents = PhotonServer.Instance.Agents.All.ToArray(),
                Project = Project,
                AssemblyFilename = AssemblyFilename,
                PreBuild = PreBuild,
                TaskName = TaskName,
                WorkDirectory = WorkDirectory,
                ContentDirectory = ContentDirectory,
                BinDirectory = BinDirectory,
                Packages = PackageClient,
                ConnectionFactory = ConnectionFactory,
                Output = contextOutput,
                ServerVariables = Variables,
                //Commit = Commit,
            };

            try {
                switch (Mode) {
                    case AgentStartModes.All:
                        await RunAll(context);
                        break;
                    case AgentStartModes.Any:
                        await RunAny(context);
                        break;
                }

                Build.IsSuccess = true;
            }
            catch (OperationCanceledException) {
                Build.IsCancelled = true;
                throw;
            }
            catch (Exception error) {
                Build.Exception = error.UnfoldMessages();
                throw;
            }
            finally {
                Build.IsComplete = true;
                Build.Duration = DateTime.UtcNow - Build.Created;
                Build.ProjectPackages = PushedProjectPackages.ToArray();
                Build.Save();

                await Build.SetOutput(Output.GetString());
                // TODO: Save alternate version with ansi characters removed
            }
        }

        protected override DomainAgentSessionHostBase OnCreateHost(ServerAgent agent)
        {
            return new DomainAgentBuildSessionHost(this, agent, TokenSource.Token);
        }

        private async Task RunAll(ServerBuildContext context)
        {
            var sessionHandleCollection = context.RegisterAgents.All(Roles);

            try {
                await sessionHandleCollection.InitializeAsync(TokenSource.Token);

                await sessionHandleCollection.RunTasksAsync(new [] {TaskName}, TokenSource.Token);
            }
            finally {
                await sessionHandleCollection.ReleaseAllAsync(TokenSource.Token);
            }
        }

        private async Task RunAny(ServerBuildContext context)
        {
            using (var sessionHandle = context.RegisterAgents.Any(Roles)) {
                try {
                    await sessionHandle.BeginAsync(TokenSource.Token);

                    await sessionHandle.RunTaskAsync(TaskName, TokenSource.Token);
                }
                finally {
                    await sessionHandle.ReleaseAsync(TokenSource.Token);
                }
            }
        }
    }
}
