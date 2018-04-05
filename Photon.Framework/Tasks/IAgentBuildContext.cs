using Photon.Framework.Projects;

namespace Photon.Framework.Tasks
{
    public interface IAgentBuildContext
    {
        //ContextAgentDefinition Agent {get;}
        Project Project {get;}
        string PackageId {get;}
        string PackageVersion {get;}
        string AssemblyFile {get;}
        string TaskName {get;}
    }
}
