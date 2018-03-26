using System.Collections.Generic;
using System.Text;

namespace Photon.Framework.Scripts
{
    public interface IServer
    {
        IEnumerable<ScriptAgent> GetAgents(params string[] roles);
    }

    public class ScriptContext
    {
        private readonly IServer server;

        public string ProjectName {get; set;}

        public string ReleaseVersion {get; set;}

        //public ContextProjectDefinition Project {get; internal set;}
        //public ContextTaskDefinition Task {get; internal set;}
        //public ContextAgentDefinition Agent {get; internal set;}
        //public ConcurrentBag<object> Artifacts {get;}
        public StringBuilder Output {get;}


        public ScriptContext(IServer server)
        {
            this.server = server;

            //Project = new ContextProjectDefinition();
            //Task = new ContextTaskDefinition();
            //Agent = new ContextAgentDefinition();
            //Artifacts = new ConcurrentBag<object>();
            Output = new StringBuilder();
        }

        public ScriptAgentCollection RegisterAgents(params string[] roles)
        {
            var agents = server.GetAgents(roles);
            return new ScriptAgentCollection(agents);
        }
    }
}
