using Photon.Framework.Projects;
using System;
using System.Linq;

namespace Photon.Framework.Scripts
{

    [Serializable]
    public class ServerBuildContext : IServerBuildContext
    {
        public Project Project {get; set;}
        public string AssemblyFile {get; set;}
        public string ScriptName {get; set;}
        public string RefSpec {get; set;}
        public string WorkDirectory {get; set;}
        public int BuildNumber {get; set;}
        public ServerAgentDefinition[] Agents {get; set;}
        //public ConcurrentBag<object> Artifacts {get;}
        public ScriptOutput Output {get; set;}


        public ServerBuildContext()
        {
            //Artifacts = new ConcurrentBag<object>();
            //Output = new ScriptOutput();
        }

        public AgentSessionHandleCollection RegisterAgents(params string[] roles)
        {
            if (Agents == null)
                throw new Exception("No agents have been defined!");

            var roleAgents = Agents
                .Where(a => a.MatchesRoles(roles))
                .Select(a => new AgentBuildSessionHandle(a));

            return new AgentSessionHandleCollection(roleAgents);
        }
    }
}
