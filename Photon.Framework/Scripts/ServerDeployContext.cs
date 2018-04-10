using Photon.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using Photon.Framework.Packages;

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

        public ProjectPackageManager ProjectPackages {get; set;}


        public AgentSessionHandleCollection RegisterAgents(params string[] roles)
        {
            if (Agents == null)
                throw new Exception("No agents have been defined!");

            var roleAgents = Agents
                .Where(a => a.MatchesRoles(roles)).ToArray();

            PrintFoundAgents(roleAgents);

            var registry = new MessageProcessorRegistry();
            registry.Scan(typeof(IFrameworkAssembly).Assembly);

            var roleAgentHandles = roleAgents
                .Select(a => new AgentDeploySessionHandle(this, a, registry));

            return new AgentSessionHandleCollection(roleAgentHandles);
        }

        private void PrintFoundAgents(IEnumerable<ServerAgentDefinition> agents)
        {
            var agentNames = agents.Select(x => x.Name);
            Output.Append("Found Agents: ", ConsoleColor.DarkCyan);

            var i = 0;
            foreach (var name in agentNames) {
                if (i > 0) Output.Append("; ", ConsoleColor.DarkCyan);
                i++;

                Output.Append(name, ConsoleColor.Cyan);
            }

            Output.AppendLine(".", ConsoleColor.DarkCyan);
        }
    }
}
