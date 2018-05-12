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
            var agents = GetAllAgents();
            var roleAgents = AgentsInRoles(agents, roles);
            var agent = GetRandomAgent(roleAgents);

            PrintFoundAgents(new[] {agent});

            var handle = ConnectTo(agent);
            context.agentSessions.Add(handle);
            return handle;
        }

        public AgentSessionHandleCollection All()
        {
            var agents = GetAllAgents().ToArray();

            PrintFoundAgents(agents);
            return CreateCollection(agents);
        }

        public AgentSessionHandleCollection All(params string[] roles)
        {
            var agents = GetAllAgents();
            var roleAgents = AgentsInRoles(agents, roles).ToArray();

            PrintFoundAgents(roleAgents);
            return CreateCollection(roleAgents);
        }

        public AgentSessionHandleCollection Environment(string name)
        {
            var agents = GetAllAgents();
            var environmentAgents = AgentsInEnvironment(agents, name).ToArray();

            PrintFoundAgents(environmentAgents);
            return CreateCollection(environmentAgents);
        }

        public AgentSessionHandleCollection Environment(string name, params string[] roles)
        {
            var allAgents = GetAllAgents();
            var environmentAgents = AgentsInEnvironment(allAgents, name);
            var roleAgents = AgentsInRoles(environmentAgents, roles).ToArray();

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

        private IEnumerable<ServerAgent> GetAllAgents()
        {
            if (context.Agents?.Any() ?? false)
                return context.Agents;

            context.Output.AppendLine("No agents have been defined!", ConsoleColor.DarkRed);
            throw new ApplicationException("No agents have been defined!");
        }

        private IEnumerable<ServerAgent> AgentsInRoles(IEnumerable<ServerAgent> agents, params string[] roles)
        {
            var roleAgents = agents.Where(a => a.MatchesRoles(roles)).ToArray();

            if (roleAgents.Any()) return roleAgents;

            context.Output.Append("No agents were found in roles ", ConsoleColor.DarkYellow);

            var i = 0;
            foreach (var role in roles) {
                if (i > 0) context.Output.Append(", ", ConsoleColor.DarkYellow);
                i++;

                context.Output.Append(role, ConsoleColor.Yellow);
            }

            context.Output.AppendLine("!", ConsoleColor.DarkYellow);

            throw new ApplicationException($"No agents were found in roles '{string.Join(", ", roles)}'!");
        }

        private IEnumerable<ServerAgent> AgentsInEnvironment(IEnumerable<ServerAgent> agents, string environmentName)
        {
            var environmentList = context.Project?.Environments;

            if (!(environmentList?.Any() ?? false)) {
                context.Output.AppendLine("No environments have been defined!", ConsoleColor.DarkRed);
                throw new ApplicationException("No environments have been defined!");
            }

            var environment = environmentList.FirstOrDefault(x =>
                string.Equals(x.Name, environmentName, StringComparison.OrdinalIgnoreCase));

            if (environment == null) {
                context.Output.Append("Environment ", ConsoleColor.DarkYellow)
                    .Append(environmentName, ConsoleColor.Yellow)
                    .AppendLine(" was not found!", ConsoleColor.DarkYellow);

                throw new ApplicationException($"Environment '{environmentName}' was not found!");
            }

            var environmentAgents = agents
                .Where(a => environment.AgentIdList.Any(x => string.Equals(x, a.Id, StringComparison.OrdinalIgnoreCase)))
                .ToArray();

            if (environmentAgents.Any())
                return environmentAgents;

            context.Output.Append("No agents were found in environment ", ConsoleColor.DarkYellow)
                .Append(environmentName, ConsoleColor.Yellow)
                .AppendLine("!", ConsoleColor.DarkYellow);

            throw new ApplicationException($"No agents were found in environment '{environmentName}'!");
        }

        private static ServerAgent GetRandomAgent(IEnumerable<ServerAgent> agents)
        {
            var _agentArray = agents as ServerAgent[] ?? agents.ToArray();

            if (_agentArray.Length <= 1)
                return _agentArray.FirstOrDefault();

            var random = new Random();
            return _agentArray[random.Next(_agentArray.Length)];
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

        private void PrintFoundAgents(IEnumerable<ServerAgent> agents)
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
