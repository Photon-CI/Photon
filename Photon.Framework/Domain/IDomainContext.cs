using Photon.Framework.Applications;
using Photon.Framework.Packages;
using Photon.Framework.Process;
using Photon.Framework.Projects;
using Photon.Framework.Variables;

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
        ProcessClient Process {get;}
    }
}
