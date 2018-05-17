using Photon.Framework.Projects;
using Photon.Framework.Server;
using Photon.Framework.Variables;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Framework.Domain
{
    public interface IDomainContext
    {
        Project Project {get;}
        string AssemblyFilename {get;}
        string WorkDirectory {get;}
        string ContentDirectory {get;}
        string BinDirectory {get;}
        ScriptOutput Output {get;}
        VariableSetCollection ServerVariables {get;}
        VariableSetCollection AgentVariables {get;}

        void RunCommandLine(string command);
        void RunCommandLine(string command, params string[] args);
        Task RunCommandLineAsync(string command);
        Task RunCommandLineAsync(string command, params string[] args);
        Task PushProjectPackageAsync(string filename, CancellationToken token);
        Task PushApplicationPackageAsync(string filename, CancellationToken token);
        Task<string> PullProjectPackageAsync(string id, string version);
        Task<string> PullApplicationPackageAsync(string id, string version);
    }
}
