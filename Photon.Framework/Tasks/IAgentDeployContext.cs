using Photon.Framework.Projects;
using Photon.Framework.Scripts;
using System.Threading.Tasks;

namespace Photon.Framework.Tasks
{
    public interface IAgentDeployContext
    {
        Project Project {get;}
        string ProjectPackageId {get;}
        string ProjectPackageVersion {get;}
        string TaskName {get;}
        string WorkDirectory {get;}
        ScriptOutput Output {get;}

        string GetApplicationDirectory(string applicationName, string applicationVersion);
        Task<string> DownloadProjectPackageAsync(string packageId, string packageVersion, string outputPath);
        Task<string> DownloadApplicationPackageAsync(string packageId, string packageVersion, string outputPath);
    }
}
