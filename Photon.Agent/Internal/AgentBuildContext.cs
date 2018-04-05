using Photon.Framework;
using Photon.Framework.Projects;
using Photon.Framework.Scripts;
using Photon.Framework.Tasks;

namespace Photon.Agent.Internal
{
    public class AgentBuildContext : IAgentBuildContext
    {
        //public ContextAgentDefinition Agent {get;}
        public Project Project {get; set;}
        public string PackageId {get; set;}
        public string PackageVersion {get; set;}
        public string AssemblyFile {get; set;}
        public string TaskName {get; set;}
        public string WorkDirectory {get; set;}
        //public ConcurrentBag<object> Artifacts {get;}
        public ScriptOutput Output {get;}


        public AgentBuildContext(AgentDefinition agent)
        {
            //Agent = new ContextAgentDefinition {
            //    Name = agent.Name,
            //    Roles = agent.Roles,
            //};

            //Artifacts = new ConcurrentBag<object>();
            Output = new ScriptOutput();
        }
    }
}
