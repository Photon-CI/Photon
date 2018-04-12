using Photon.Framework;
using Photon.Framework.Packages;
using Photon.Framework.Projects;
using Photon.Framework.Server;
using Photon.Framework.Tasks;
using System.IO;
using System.Threading.Tasks;

namespace Photon.Server.Internal.Sessions
{
    internal class ServerDeploySession : ServerSessionBase
    {
        public Project Project {get; set;}
        public string AssemblyFilename {get; set;}
        public string ScriptName {get; set;}
        public string ProjectPackageId {get; set;}
        public string ProjectPackageVersion {get; set;}
        public string ProjectPackageFilename {get; set;}


        protected override DomainAgentSessionHostBase OnCreateHost(ServerAgentDefinition agent)
        {
            return new DomainAgentDeploySessionHost(this, agent);
        }

        public override async Task PrepareWorkDirectoryAsync()
        {
            await base.PrepareWorkDirectoryAsync();

            var metadata = await ProjectPackageTools.UnpackAsync(ProjectPackageFilename, BinDirectory);

            AssemblyFilename = metadata.AssemblyFilename;
            ScriptName = metadata.ScriptName;
        }

        public override async Task<TaskResult> RunAsync()
        {
            var assemblyFilename = Path.Combine(BinDirectory, AssemblyFilename);
            if (!File.Exists(assemblyFilename))
                throw new FileNotFoundException($"The assembly file '{assemblyFilename}' could not be found!");

            Domain = new ServerDomain();
            Domain.Initialize(assemblyFilename);

            var context = new ServerDeployContext {
                Agents = PhotonServer.Instance.Definition.Agents.ToArray(),
                ProjectPackageId = ProjectPackageId,
                ProjectPackageVersion = ProjectPackageVersion,
                ScriptName = ScriptName,
                WorkDirectory = WorkDirectory,
                BinDirectory = BinDirectory,
                ContentDirectory = ContentDirectory,
                Packages = PackageClient,
                ConnectionFactory = ConnectionFactory,
                Output = Output,
            };

            return await Domain.RunDeployScript(context);
        }
    }
}
