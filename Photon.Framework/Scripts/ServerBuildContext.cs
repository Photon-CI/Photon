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


        public AgentSessionHandleCollection RegisterAgents(params string[] roles)
        {
            if (Agents == null)
                throw new Exception("No agents have been defined!");

            var roleAgents = Agents.Where(a => a.MatchesRoles(roles)).ToArray();

            var agentNames = roleAgents.Select(x => x.Name);
            Output.Append("Registering Agents: ", ConsoleColor.DarkCyan)
                .AppendLine(string.Join("; ", agentNames));

            var roleAgentHandles = roleAgents.Select(a => new AgentBuildSessionHandle(this, a));

            return new AgentSessionHandleCollection(roleAgentHandles);
        }
    }
}
