using Photon.Framework.Projects;

namespace Photon.Framework.Tasks
{
    public interface IAgentBuildContext
    {
        Project Project {get;}
        string AssemblyFile {get;}
        string TaskName {get;}
        string GitRefspec {get;}
        string WorkDirectory {get;}
        ITaskOutput Output {get;}
    }
}
