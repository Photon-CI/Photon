using Photon.Framework.Projects;
using Photon.Framework.Server;
using System.Threading.Tasks;

namespace Photon.Server.Internal.Sessions
{
    internal class ServerBuildSession : ServerSessionBase
    {
        private IAgentSessionHandle sessionHandle;

        public Project Project {get; set;}
        public string AssemblyFile {get; set;}
        public string TaskName {get; set;}
        public string GitRefspec {get; set;}
        public string[] Roles {get; set;}
        public int BuildNumber {get; set;}


        public override async Task RunAsync()
        {
            var context = new ServerBuildContext {
                Agents = PhotonServer.Instance.Definition.Agents.ToArray(),
                ProjectPackages = PhotonServer.Instance.ProjectPackages,
                ApplicationPackages = PhotonServer.Instance.ApplicationPackages,
                Project = Project,
                AssemblyFilename = AssemblyFile,
                TaskName = TaskName,
                BuildNumber = BuildNumber,
                WorkDirectory = WorkDirectory,
                ContentDirectory = ContentDirectory,
                BinDirectory = BinDirectory,
                Output = Output,
            };

            using (sessionHandle = context.RegisterAnyAgent(Roles)) {
                try {
                    await sessionHandle.BeginAsync();

                    await sessionHandle.RunTaskAsync(TaskName);
                }
                finally {
                    await sessionHandle.ReleaseAsync();
                }
            }
        }
    }
}
