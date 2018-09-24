using Photon.Framework.Extensions;
using Photon.Framework.Server;
using Photon.Library.GitHub;
using Photon.Library.Http.Messages;
using Photon.Server.Internal.Builds;
using Photon.Server.Internal.New;
using System;
using System.Linq;
using System.Threading.Tasks;
using Photon.Framework.AgentConnection;
using Photon.Server.Internal.AgentConnections;

namespace Photon.Server.Internal.Sessions
{
    internal class ServerBuildSession : ServerSessionBase
    {
        public BuildData Build {get; set;}
        public string AssemblyFilename {get; set;}
        public string PreBuild {get; set;}
        public string TaskName {get; set;}
        public string GitRefspec {get; set;}
        public string[] Roles {get; set;}
        public AgentStartModes Mode {get; set;}
        public GithubCommit Commit {get; set;}


        public ServerBuildSession(ServerContext context) : base(context)
        {
            //
        }

        public override async Task RunAsync()
        {
            //var contextOutput = new DomainOutput();
            //contextOutput.OnWrite += (text, color) => Output.Write(text, color);
            //contextOutput.OnWriteLine += (text, color) => Output.WriteLine(text, color);
            //contextOutput.OnWriteRaw += text => Output.WriteRaw(text);

            //var packageClient = new DomainPackageClient(Packages.Client);

            //var context = new ServerBuildContext {
            //    BuildNumber = Build.Number,
            //    Agents = PhotonServer.Instance.Agents.All.ToArray(),
            //    Project = Project,
            //    AssemblyFilename = AssemblyFilename,
            //    PreBuild = PreBuild,
            //    TaskName = TaskName,
            //    WorkDirectory = WorkDirectory,
            //    ContentDirectory = ContentDirectory,
            //    BinDirectory = BinDirectory,
            //    Packages = packageClient,
            //    ConnectionFactory = ConnectionFactory,
            //    Output = contextOutput,
            //    ServerVariables = Variables,
            //    Commit = Commit,
            //};

            try {
                switch (Mode) {
                    case AgentStartModes.All:
                        await RunAll();
                        break;
                    case AgentStartModes.Any:
                        await RunAny();
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
                Build.ProjectPackages = Packages.PushedProjectPackages.ToArray();
                Build.ApplicationPackages = Packages.PushedApplicationPackages.ToArray();
                Build.Save();

                await Build.SetOutput(Output.GetString());
                // TODO: Save alternate version with ansi characters removed
            }
        }

        protected override DomainAgentSessionHostBase OnCreateHost(ServerAgent agent)
        {
            return new DomainAgentBuildSessionHost(this, agent, TokenSource.Token);
        }

        private async Task RunAll()
        {
            var selector = new ServerAgentSelector(ConnectionFactory) {
                Agents = PhotonServer.Instance.Agents.All.ToArray(),
                Project = Project,
            };

            var sessionHandleCollection = selector.All(Roles);

            try {
                await sessionHandleCollection.BeginAsync(TokenSource.Token);

                await sessionHandleCollection.RunTasksAsync(new [] {TaskName}, TokenSource.Token);
            }
            finally {
                await sessionHandleCollection.ReleaseAsync(TokenSource.Token);
            }
        }

        private async Task RunAny()
        {
            var selector = new ServerAgentSelector(ConnectionFactory) {
                Agents = PhotonServer.Instance.Agents.All.ToArray(),
                Project = Project,
            };

            using (var sessionHandle = selector.Any(Roles)) {
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
