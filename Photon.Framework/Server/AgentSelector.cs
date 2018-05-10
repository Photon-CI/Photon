using Photon.Framework.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Photon.Framework.Server
{
    public class AgentSelector
    {
        private readonly ServerContextBase context;


        public AgentSelector(ServerContextBase context)
        {
            this.context = context;
        }

        public DomainAgentSessionHandle Any(params string[] roles)
        {
            AssertAgentsAreDefined();

            var roleAgents = context.Agents
                .Where(a => a.MatchesRoles(roles)).ToArray();

            AssertAnyAgents(roleAgents, roles);

            ServerAgent agent;
            if (roleAgents.Length <= 1) {
                agent = roleAgents.First();
            }
            else {
                var random = new Random();
                agent = roleAgents[random.Next(roleAgents.Length)];
            }

            PrintFoundAgents(agent);

            var handle = ConnectTo(agent);
            context.agentSessions.Add(handle);
            return handle;
        }

        public AgentSessionHandleCollection All()
        {
            AssertAgentsAreDefined();

            PrintFoundAgents(context.Agents);
            return CreateCollection(context.Agents);
        }

        public AgentSessionHandleCollection All(params string[] roles)
        {
            AssertAgentsAreDefined();

            var roleAgents = context.Agents
                .Where(a => a.MatchesRoles(roles)).ToArray();

            AssertAnyAgents(roleAgents, roles);

            PrintFoundAgents(roleAgents);
            return CreateCollection(roleAgents);
        }

        public AgentSessionHandleCollection Environment(string name)
        {
            AssertAgentsAreDefined();

            var environment = context.Project.Environments
                .FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));

            if (environment == null) {
                context.Output.Append("Environment ", ConsoleColor.DarkYellow)
                    .Append(name, ConsoleColor.Yellow)
                    .AppendLine(" was not found!", ConsoleColor.DarkYellow);

                throw new ApplicationException($"Environment '{name}' was not found!");
            }

            var environmentAgents = context.Agents
                .Where(a => environment.AgentIdList.Any(x => string.Equals(x, a.Id, StringComparison.OrdinalIgnoreCase)))
                .ToArray();

            if (!environmentAgents.Any()) {
                context.Output.Append("No agents were found in environment ", ConsoleColor.DarkYellow)
                    .Append(name, ConsoleColor.Yellow)
                    .AppendLine("!", ConsoleColor.DarkYellow);

                throw new ApplicationException($"No agents were found in environment '{name}'!");
            }

            PrintFoundAgents(environmentAgents);
            return CreateCollection(environmentAgents);
        }

        public AgentSessionHandleCollection Environment(string name, params string[] roles)
        {
            AssertAgentsAreDefined();

            var environment = context.Project.Environments
                .FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));

            if (environment == null) {
                context.Output.Append("Environment ", ConsoleColor.DarkYellow)
                    .Append(name, ConsoleColor.Yellow)
                    .AppendLine(" was not found!", ConsoleColor.DarkYellow);

                throw new ApplicationException($"Environment '{name}' was not found!");
            }

            var environmentAgents = context.Agents
                .Where(a => environment.AgentIdList.Any(x => string.Equals(x, a.Id, StringComparison.OrdinalIgnoreCase)))
                .ToArray();

            var roleAgents = environmentAgents
                .Where(a => a.MatchesRoles(roles)).ToArray();

            AssertAnyAgents(roleAgents, roles);

            PrintFoundAgents(roleAgents);
            return CreateCollection(roleAgents);
        }

        public DomainAgentSessionHandle ConnectTo(ServerAgent agent)
        {
            var sessionClient = context.ConnectionFactory.RequestConnection(agent);

            var sessionHandle = new DomainAgentSessionHandle(sessionClient);
            context.agentSessions.Add(sessionHandle);
            return sessionHandle;
        }

        private void AssertAgentsAreDefined()
        {
            if (context.Agents?.Any() ?? false) return;

            context.Output.AppendLine("No agents have been defined!", ConsoleColor.DarkRed);
            throw new ApplicationException("No agents have been defined!");
        }

        private void AssertAnyAgents(IEnumerable<ServerAgent> agents, string[] roles)
        {
            if (agents.Any()) return;

            PrintNotFoundAgents(roles);
            throw new ApplicationException($"No agents were found in roles '{string.Join(", ", roles)}'!");
        }

        private AgentSessionHandleCollection CreateCollection(IEnumerable<ServerAgent> agents)
        {
            var roleAgentHandles = agents.Select(a => {
                var handle = ConnectTo(a);
                context.agentSessions.Add(handle);
                return handle;
            });

            return new AgentSessionHandleCollection(context, roleAgentHandles);
        }

        private void PrintNotFoundAgents(string[] roles)
        {
            context.Output.Append("No agents were found in roles ", ConsoleColor.DarkYellow);

            var i = 0;
            foreach (var role in roles) {
                if (i > 0) context.Output.Append(", ", ConsoleColor.DarkYellow);
                i++;

                context.Output.Append(role, ConsoleColor.Yellow);
            }

            context.Output.AppendLine("!", ConsoleColor.DarkYellow);
        }

        private void PrintFoundAgents(params ServerAgent[] agents)
        {
            var agentNames = agents.Select(x => x.Name);
            context.Output.Append("Found Agents: ", ConsoleColor.DarkCyan);

            var i = 0;
            foreach (var name in agentNames) {
                if (i > 0) context.Output.Append("; ", ConsoleColor.DarkCyan);
                i++;

                context.Output.Append(name, ConsoleColor.Cyan);
            }

            context.Output.AppendLine(".", ConsoleColor.DarkCyan);
        }
    }
}
