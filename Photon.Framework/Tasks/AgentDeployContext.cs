using Photon.Framework.Projects;
using Photon.Framework.Scripts;
using System;
using System.Threading.Tasks;

namespace Photon.Framework.Tasks
{
    public class AgentDeployContext : MarshalByRefObject, IAgentDeployContext
    {
        public Project Project {get; set;}
        public string ProjectPackageId {get; set;}
        public string ProjectPackageVersion {get; set;}
        public string AssemblyFilename {get; set;}
        public string TaskName {get; set;}
        public string WorkDirectory {get; set;}
        public string ContentDirectory {get; set;}
        public string BinDirectory {get; set;}
        public ScriptOutput Output {get; set;}


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
