using Photon.Framework.Projects;
using Photon.Framework.Server;
using Photon.Library.GitHub;
using System.Linq;
using System.Threading.Tasks;

namespace Photon.Server.Internal.Sessions
{
    internal class ServerBuildSession : ServerSessionBase
    {
        public uint BuildNumber {get; set;}
        public Project Project {get; set;}
        public string AssemblyFilename {get; set;}
        public string PreBuild {get; set;}
        public string TaskName {get; set;}
        public string GitRefspec {get; set;}
        public string[] Roles {get; set;}
        public GithubCommit Commit {get; set;}


        public override async Task RunAsync()
        {
            var context = new ServerBuildContext {
                BuildNumber = BuildNumber,
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
                Output = Output,
                ServerVariables = Variables,
                //Commit = Commit,
            };

            using (var sessionHandle = context.RegisterAnyAgent(Roles)) {
                try {
                    await sessionHandle.BeginAsync(TokenSource.Token);

                    await sessionHandle.RunTaskAsync(TaskName, TokenSource.Token);
                }
                finally {
                    await sessionHandle.ReleaseAsync(TokenSource.Token);
                }
            }
        }

        protected override DomainAgentSessionHostBase OnCreateHost(ServerAgent agent)
        {
            return new DomainAgentBuildSessionHost(this, agent, TokenSource.Token);
        }
    }
}
