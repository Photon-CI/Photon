using Photon.Framework.Extensions;
using Photon.Library.Communication.Messages.Session;
using Photon.Library.GitHub;
using Photon.Library.Http.Messages;
using Photon.Server.Internal.AgentConnections;
using Photon.Server.Internal.Builds;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Photon.Server.Internal.Sessions
{
    internal class ServerBuildSession : ServerSessionBase
    {
        public event EventHandler PreBuildEvent;
        public event EventHandler PostBuildEvent;

        public BuildData Build {get; set;}
        public string AssemblyFilename {get; set;}
        public string PreBuild {get; set;}
        public string TaskName {get; set;}
        public string GitRefspec {get; set;}
        public string[] Roles {get; set;}
        public AgentStartModes Mode {get; set;}
        public GithubCommit Commit {get; set;}


        public ServerBuildSession(ServerContext context) : base(context) {}

        public override async Task RunAsync()
        {
            try {
                OnPreBuildEvent();
            }
            catch (Exception error) {
                Log.Error("Pre-Build event failed!", error);
            }

            try {
                var sessionHandleCollection = await GetAgentCollection();

                await Run(sessionHandleCollection);

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
                Build.ApplicationPackages = PushedApplicationPackages.ToArray();
                Build.Save();

                await Build.SetOutput(Output.GetString());
                // TODO: Save alternate version with ansi characters removed

                try {
                    OnPostBuildEvent();
                }
                catch (Exception error) {
                    Log.Error("Post-Build event failed!", error);
                }
            }
        }

        private async Task<ServerConnectionCollection> GetAgentCollection()
        {
            var selector = new ServerAgentSelector(ConnectionFactory) {
                Agents = PhotonServer.Instance.Agents.All.ToArray(),
                Project = Project,
            };

            switch (Mode) {
                case AgentStartModes.All:
                    return await selector.GetAllAsync(Roles);
                case AgentStartModes.Any:
                    return await selector.GetAnyAsync(Roles);
                default:
                    throw new ApplicationException($"Invalid Agent Start-Mode '{Mode}'!");
            }
        }

        private async Task Run(ServerConnectionCollection sessionHandleCollection)
        {
            try {
                //await sessionHandleCollection.BeginAsync(TokenSource.Token);

                await sessionHandleCollection.RunAsync(async connection => {
                    var request = new AgentBuildSessionRunRequest {
                        ServerSessionId = SessionId,
                        BuildNumber = Build.Number,
                        //Agents = PhotonServer.Instance.Agents.All.ToArray(),
                        Project = Project,
                        AssemblyFilename = AssemblyFilename,
                        PreBuildCommand = PreBuild,
                        TaskName = TaskName,
                        //WorkDirectory = WorkDirectory,
                        //ContentDirectory = ContentDirectory,
                        //BinDirectory = BinDirectory,
                        ServerVariables = await PhotonServer.Instance.Variables.GetCollection(),
                        GitRefspec = GitRefspec,
                        CommitHash = Commit?.Sha,
                        //CommitAuthor = ?,
                        //CommitMessage = ?,
                    };

                    var response = await connection.Transceiver.Send(request)
                        .GetResponseAsync<AgentBuildSessionRunResponse>();

                    // TODO
                });
            }
            finally {
                sessionHandleCollection.Release();
            }
        }

        private void OnPreBuildEvent()
        {
            PreBuildEvent?.Invoke(this, EventArgs.Empty);
        }

        private void OnPostBuildEvent()
        {
            PostBuildEvent?.Invoke(this, EventArgs.Empty);
        }
    }
}
