using Photon.Framework.Projects;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Photon.Framework.Scripts
{
    public interface IServer
    {
        string WorkDirectory {get;}
        IEnumerable<ScriptAgent> GetAgents(params string[] roles);
    }

    public class ScriptContext
    {
        private readonly IServer server;

        public string SessionId {get;}
        public string WorkDirectory {get;}
        public string ReleaseVersion {get; set;}
        public ProjectDefinition Project {get; internal set;}
        public ProjectJobDefinition Job {get; internal set;}
        //public ContextAgentDefinition Agent {get; internal set;}
        //public ConcurrentBag<object> Artifacts {get;}
        public StringBuilder Output {get;}


        public ScriptContext(string sessionId, IServer server, ProjectDefinition project, ProjectJobDefinition job)
        {
            this.SessionId = sessionId;
            this.server = server;
            this.Project = project;
            this.Job = job;

            //Agent = new ContextAgentDefinition();
            //Artifacts = new ConcurrentBag<object>();
            Output = new StringBuilder();

            WorkDirectory = Path.Combine(server.WorkDirectory, sessionId);
        }

        public ScriptAgentCollection RegisterAgents(params string[] roles)
        {
            var agents = server.GetAgents(roles);
            return new ScriptAgentCollection(agents);
        }
    }
}
