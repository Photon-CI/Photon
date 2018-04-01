using Photon.Framework.Projects;
using System;
using System.Linq;

namespace Photon.Framework.Scripts
{
    [Serializable]
    public class ScriptContext
    {
        public string SessionId {get;}
        public AgentDefinition[] Agents {get;}
        public string ReleaseVersion {get; set;}
        public string WorkDirectory {get; set;}
        public ProjectDefinition Project {get; internal set;}
        public ProjectJobDefinition Job {get; internal set;}
        //public ConcurrentBag<object> Artifacts {get;}
        public ScriptOutput Output {get;}


        public ScriptContext(string sessionId, ProjectDefinition project, ProjectJobDefinition job, AgentDefinition[] agents)
        {
            this.SessionId = sessionId ?? throw new ArgumentNullException(nameof(sessionId));
            this.Project = project ?? throw new ArgumentNullException(nameof(project));
            this.Job = job ?? throw new ArgumentNullException(nameof(job));
            this.Agents = agents ?? throw new ArgumentNullException(nameof(agents));

            //Artifacts = new ConcurrentBag<object>();
            Output = new ScriptOutput();
        }

        public ScriptAgentCollection RegisterAgents(params string[] roles)
        {
            var roleAgents = Agents
                .Where(a => a.MatchesRoles(roles))
                .Select(a => new ScriptAgent(a));

            return new ScriptAgentCollection(roleAgents);
        }
    }
}
