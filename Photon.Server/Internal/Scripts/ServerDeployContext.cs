using Photon.Framework;
using Photon.Framework.Projects;
using Photon.Framework.Scripts;
using System;
using System.Linq;

namespace Photon.Server.Internal.Scripts
{

    [Serializable]
    public class ServerDeployContext : IServerDeployContext
    {
        public Project Project {get; set;}
        public string AssemblyFile {get; set;}
        public string ScriptName {get; set;}
        public string WorkDirectory {get; set;}
        public ServerAgentDefinition[] Agents {get; set;}
        //public ConcurrentBag<object> Artifacts {get;}
        public ScriptOutput Output {get; set;}


        public ServerDeployContext()
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
                .Select(a => new AgentDeploySessionHandle(a));

            return new AgentSessionHandleCollection(roleAgents);
        }
    }
}
