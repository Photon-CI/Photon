using Photon.Framework.Projects;
using Photon.Framework.Server;

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
    }
}
