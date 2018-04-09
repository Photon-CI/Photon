using System;
using System.Linq;

namespace Photon.Framework.Scripts
{

    [Serializable]
    public class ServerDeployContext : IServerDeployContext
    {
        public string ProjectPackageId {get; set;}
        public string ProjectPackageVersion {get; set;}
        public string AssemblyFile {get; set;}
        public string ScriptName {get; set;}
        public string WorkDirectory {get; set;}
        public string BinDirectory {get; set;}
        public string ContentDirectory {get; set;}
        public ServerAgentDefinition[] Agents {get; set;}
        public ScriptOutput Output {get; set;}


        public AgentSessionHandleCollection RegisterAgents(params string[] roles)
        {
            if (Agents == null)
                throw new Exception("No agents have been defined!");

            var roleAgents = Agents
                .Where(a => a.MatchesRoles(roles))
                .Select(a => new AgentDeploySessionHandle(a));

            return new AgentSessionHandleCollection(roleAgents);
        }
    }
}
