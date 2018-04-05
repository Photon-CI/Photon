using Photon.Framework.Projects;

namespace Photon.Framework.Tasks
{
    public interface IDeployTaskContext
    {
        Project Project {get;}
        //ContextAgentDefinition Agent {get;}
        string TaskName {get;}
    }
}
