using Photon.Framework;
using Photon.Framework.Projects;
using Photon.Framework.Scripts;
using Photon.Framework.Tasks;
using System;
using System.Threading.Tasks;

namespace Photon.Agent.Internal.Tasks
{
    public class AgentDeployContext : IAgentDeployContext
    {
        public Project Project {get; set;}
        //public ContextAgentDefinition Agent {get; set;}
        public string ProjectId {get; set;}
        public string ProjectVersion {get; set;}
        public string TaskName {get; set;}
        public string WorkDirectory {get; set;}
        //public ConcurrentBag<object> Artifacts {get;}
        public ScriptOutput Output {get;}


        public AgentDeployContext(AgentDefinition agent)
        {
            //Agent = new ContextAgentDefinition {
            //    Name = agent.Name,
            //    Roles = agent.Roles,
            //};

            //Artifacts = new ConcurrentBag<object>();
            Output = new ScriptOutput();
        }

        public string GetApplicationDirectory(string applicationName, string applicationVersion)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Downloads a Project package from the Server to the specified path.
        /// </summary>
        /// <param name="packageId">The ID of the package.</param>
        /// <param name="packageVersion">The version of the package.</param>
        /// <param name="outputPath">The local path to download the file into.</param>
        /// <returns>The filename of the downloaded package.</returns>
        public async Task<string> DownloadProjectPackageAsync(string packageId, string packageVersion, string outputPath)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Downloads an Application package from the Server to the specified path.
        /// </summary>
        /// <param name="packageId">The ID of the package.</param>
        /// <param name="packageVersion">The version of the package.</param>
        /// <param name="outputPath">The local path to download the file into.</param>
        /// <returns>The filename of the downloaded package.</returns>
        public async Task<string> DownloadApplicationPackageAsync(string packageId, string packageVersion, string outputPath)
        {
            throw new NotImplementedException();
        }
    }
}
