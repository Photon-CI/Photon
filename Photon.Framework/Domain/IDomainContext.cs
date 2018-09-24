using Photon.Framework.Packages;
using Photon.Framework.Process;
using Photon.Framework.Projects;
using Photon.Framework.Variables;

namespace Photon.Framework.Domain
{
    public interface IDomainContext
    {
        string ServerSessionId {get;}
        Project Project {get;}
        string AssemblyFilename {get;}
        string WorkDirectory {get;}
        string ContentDirectory {get;}
        string BinDirectory {get;}
        IWriteBlocks Output {get;}
        VariableSetCollection ServerVariables {get;}
        VariableSetCollection AgentVariables {get;}
        IPackageClient Packages {get;}
        ProcessClient Process {get;}
    }
}
