using Photon.Framework.Projects;
using Photon.Framework.Server;
using System.Threading.Tasks;

namespace Photon.Framework.Agent
{
    public interface IAgentContext
    {
        Project Project {get;}
        string AssemblyFilename {get;}
        string WorkDirectory {get;}
        string ContentDirectory {get;}
        string BinDirectory {get;}
        ScriptOutput Output {get;}

        void RunCommandLine(string command);
        void RunCommandLine(string command, params string[] args);
        Task PushProjectPackageAsync(string filename);
        Task PushApplicationPackageAsync(string filename);
        Task PullProjectPackageAsync(string id, string version, string filename);
        Task PullApplicationPackageAsync(string id, string version, string filename);
    }
}
