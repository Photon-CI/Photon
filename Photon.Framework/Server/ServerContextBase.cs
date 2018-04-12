using Photon.Communication;
using Photon.Framework.Packages;
using Photon.Framework.Projects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Photon.Framework.Server
{
    [Serializable]
    public abstract class ServerContextBase : IServerContext
    {
        public ServerAgentDefinition[] Agents {get; set;}

        [NonSerialized]
        private List<IAgentSessionHandle> agentSessions;

        public ProjectPackageManager ProjectPackages {get; set;}
        public ApplicationPackageManager ApplicationPackages {get; set;}

        public string ServerSessionId {get; set;}
        public Project Project {get; set;}
        public string AssemblyFilename {get; set;}
        public string WorkDirectory {get; set;}
        public string BinDirectory {get; set;}
        public string ContentDirectory {get; set;}
        public ScriptOutput Output {get; set;}


        protected ServerContextBase()
        {
            agentSessions = new List<IAgentSessionHandle>();
        }

        [OnDeserializing]
        private void OnDeserializing(StreamingContext c)
        {
            agentSessions = new List<IAgentSessionHandle>();
        }

        public virtual void Dispose()
        {
            foreach (var session in agentSessions)
                session.Dispose();

            agentSessions.Clear();
        }

        public IAgentSessionHandle GetAgentSession(string sessionId)
        {
            return agentSessions.FirstOrDefault(x => string.Equals(x.AgentSessionId, sessionId));
        }

        public IAgentSessionHandle RegisterAnyAgent(params string[] roles)
        {
            if (Agents == null)
                throw new Exception("No agents have been defined!");

            var roleAgents = Agents
                .Where(a => a.MatchesRoles(roles)).ToArray();

            ServerAgentDefinition agent;
            if (roleAgents.Length <= 1) {
                agent = roleAgents.FirstOrDefault();
            }
            else {
                var random = new Random();
                agent = roleAgents[random.Next(roleAgents.Length)];
            }

            PrintFoundAgents(agent);

            var registry = new MessageProcessorRegistry();
            registry.Scan(typeof(IFrameworkAssembly).Assembly);

            var handle = CreateSessionHandle(agent, registry);

            agentSessions.Add(handle);

            return handle;
        }

        public AgentSessionHandleCollection RegisterAllAgents(params string[] roles)
        {
            if (Agents == null)
                throw new Exception("No agents have been defined!");

            var roleAgents = Agents
                .Where(a => a.MatchesRoles(roles)).ToArray();

            PrintFoundAgents(roleAgents);

            var registry = new MessageProcessorRegistry();
            registry.Scan(typeof(IFrameworkAssembly).Assembly);

            var roleAgentHandles = roleAgents.Select(a => {
                var handle = CreateSessionHandle(a, registry);
                agentSessions.Add(handle);
                return handle;
            });

            return new AgentSessionHandleCollection(this, roleAgentHandles);
        }

        protected abstract IAgentSessionHandle CreateSessionHandle(ServerAgentDefinition agent, MessageProcessorRegistry registry);

        private void PrintFoundAgents(params ServerAgentDefinition[] agents)
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
