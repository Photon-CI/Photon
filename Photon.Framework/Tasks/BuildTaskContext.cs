using Photon.Framework.Projects;

namespace Photon.Framework.Tasks
{
    public class BuildTaskContext : TaskContextBase
    {
        public ProjectDefinition Project {get; set;}
        public ProjectJobDefinition Job {get; set;}
        public ContextAgentDefinition Agent {get; set;}
        public string TaskName {get; set;}


        public BuildTaskContext(AgentDefinition agent, string sessionId) : base(sessionId) // IAgent agent
        {
            //this.Project = project ?? throw new ArgumentNullException(nameof(project));
            //this.Job = job ?? throw new ArgumentNullException(nameof(job));
            //this.Agent = agent ?? throw new ArgumentNullException(nameof(agents));

            Agent = new ContextAgentDefinition {
                Name = agent.Name,
                Roles = agent.Roles,
            };
        }
    }
}
