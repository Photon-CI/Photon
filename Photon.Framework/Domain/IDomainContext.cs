using Photon.Framework.Applications;
using Photon.Framework.Packages;
using Photon.Framework.Projects;
using Photon.Framework.Variables;
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
        DomainOutput Output {get;}
        VariableSetCollection ServerVariables {get;}
        VariableSetCollection AgentVariables {get;}
        ApplicationManagerClient Applications {get;}
        DomainPackageClient Packages {get;}

        void RunCommandLine(string command);
        void RunCommandLine(string command, params string[] args);
        Task RunCommandLineAsync(string command);
        Task RunCommandLineAsync(string command, params string[] args);
    }
}
