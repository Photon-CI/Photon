using Photon.Framework.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Photon.Framework.Server
{
    [Serializable]
    public abstract class ServerContextBase : DomainContextBase, IServerContext
    {
        public ServerAgentDefinition[] Agents {get; set;}
        public DomainConnectionFactory ConnectionFactory {get; set;}

        [NonSerialized]
        private List<DomainAgentSessionHandle> agentSessions;

        public string ServerSessionId {get; set;}


        protected ServerContextBase()
        {
            agentSessions = new List<DomainAgentSessionHandle>();
        }

        [OnDeserializing]
        private void OnDeserializing(StreamingContext c)
        {
            agentSessions = new List<DomainAgentSessionHandle>();
        }

        public virtual void Dispose()
        {
            foreach (var session in agentSessions)
                session.Dispose();

            agentSessions.Clear();
        }

        public DomainAgentSessionHandle ConnectToAgent(ServerAgentDefinition agent)
        {
            var sessionClient = ConnectionFactory.RequestConnection(agent);

            var sessionHandle = new DomainAgentSessionHandle(sessionClient);
            agentSessions.Add(sessionHandle);
            return sessionHandle;
        }

        public DomainAgentSessionHandle RegisterAnyAgent(params string[] roles)
        {
            if (Agents == null) {
                Output.AppendLine("No agents are defined!", ConsoleColor.DarkRed);
                throw new ApplicationException("No agents have been defined!");
            }

            var roleAgents = Agents
                .Where(a => a.MatchesRoles(roles)).ToArray();

            if (!roleAgents.Any()) {
                PrintNotFoundAgents(roles);
                throw new ApplicationException($"No agents were found in roles '{string.Join(", ", roles)}'!");
            }

            ServerAgentDefinition agent;
            if (roleAgents.Length <= 1) {
                agent = roleAgents.First();
            }
            else {
                var random = new Random();
                agent = roleAgents[random.Next(roleAgents.Length)];
            }

            PrintFoundAgents(agent);

            var handle = ConnectToAgent(agent);
            agentSessions.Add(handle);
            return handle;
        }

        public AgentSessionHandleCollection RegisterAllAgents()
        {
            if (Agents == null)
                throw new ApplicationException("No agents have been defined!");

            PrintFoundAgents(Agents);

            var roleAgentHandles = Agents.Select(a => {
                var handle = ConnectToAgent(a);
                agentSessions.Add(handle);
                return handle;
            });

            return new AgentSessionHandleCollection(this, roleAgentHandles);
        }

        public AgentSessionHandleCollection RegisterAllAgents(params string[] roles)
        {
            if (Agents == null)
                throw new ApplicationException("No agents have been defined!");

            var roleAgents = Agents
                .Where(a => a.MatchesRoles(roles)).ToArray();

            PrintFoundAgents(roleAgents);

            var roleAgentHandles = roleAgents.Select(a => {
                var handle = ConnectToAgent(a);
                agentSessions.Add(handle);
                return handle;
            });

            return new AgentSessionHandleCollection(this, roleAgentHandles);
        }

        private void PrintNotFoundAgents(params string[] roles)
        {
            Output.Append("No agents were found in roles ", ConsoleColor.DarkYellow);

            var i = 0;
            foreach (var role in roles) {
                if (i > 0) Output.Append(", ", ConsoleColor.DarkYellow);
                Output.Append(role, ConsoleColor.Yellow);
            }

            Output.AppendLine("!", ConsoleColor.DarkYellow);
        }

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
