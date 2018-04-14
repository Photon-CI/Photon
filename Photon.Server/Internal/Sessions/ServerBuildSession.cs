using Photon.Framework;
using Photon.Framework.Projects;
using Photon.Framework.Tasks;
using System.Threading.Tasks;

namespace Photon.Server.Internal.Sessions
{
    internal class ServerBuildSession : ServerSessionBase
    {
        public Project Project {get; set;}
        public string AssemblyFilename {get; set;}
        public string PreBuild {get; set;}
        public string TaskName {get; set;}
        public string GitRefspec {get; set;}
        public string[] Roles {get; set;}
        public int BuildNumber {get; set;}


        public override async Task<TaskResult> RunAsync()
        {
            var context = new ServerBuildContext {
                Agents = PhotonServer.Instance.Definition.Agents.ToArray(),
                Project = Project,
                AssemblyFilename = AssemblyFilename,
                PreBuild = PreBuild,
                TaskName = TaskName,
                BuildNumber = BuildNumber,
                WorkDirectory = WorkDirectory,
                ContentDirectory = ContentDirectory,
                BinDirectory = BinDirectory,
                Packages = PackageClient,
                ConnectionFactory = ConnectionFactory,
                Output = Output,
            };

            using (var sessionHandle = context.RegisterAnyAgent(Roles)) {
                try {
                    await sessionHandle.BeginAsync();

                    return await sessionHandle.RunTaskAsync(TaskName);
                }
                finally {
                    await sessionHandle.ReleaseAsync();
                }
            }
        }

        protected override DomainAgentSessionHostBase OnCreateHost(ServerAgentDefinition agent)
        {
            return new DomainAgentBuildSessionHost(this, agent);
        }
    }
}
