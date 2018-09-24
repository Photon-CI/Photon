using Photon.Framework.AgentConnection;
using Photon.Framework.Projects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Photon.Framework.Server
{
    public class WorkerAgentSelector
    {
        //private readonly ServerContextBase context;
        private readonly IAgentConnectionClient connectionClient;

        public Project Project {get; set;}
        public ServerAgent[] Agents {get; set;}


        public WorkerAgentSelector(IAgentConnectionClient connectionClient)
        {
            this.connectionClient = connectionClient;
            //this.context = context;
        }

        public IAgentConnection Any(params string[] roles)
        {
            var agents = GetAllAgents();
            var roleAgents = AgentsInRoles(agents, roles);
            var agent = GetRandomAgent(roleAgents);

            //PrintFoundAgents(new[] {agent});

            var handle = ConnectTo(agent);
            //context.agentConnections.Add(handle);
            return handle;
        }

        public WorkerAgentConnectionCollection All()
        {
            var agents = GetAllAgents().ToArray();

            //PrintFoundAgents(agents);
            return CreateCollection(agents);
        }

        public WorkerAgentConnectionCollection All(params string[] roles)
        {
            var agents = GetAllAgents();
            var roleAgents = AgentsInRoles(agents, roles).ToArray();

            //PrintFoundAgents(roleAgents);
            return CreateCollection(roleAgents);
        }

        public WorkerAgentConnectionCollection Environment(string name)
        {
            var agents = GetAllAgents();
            var environmentAgents = AgentsInEnvironment(agents, name).ToArray();

            //PrintFoundAgents(environmentAgents);
            return CreateCollection(environmentAgents);
        }

        public WorkerAgentConnectionCollection Environment(string name, params string[] roles)
        {
            var allAgents = GetAllAgents();
            var environmentAgents = AgentsInEnvironment(allAgents, name);
            var roleAgents = AgentsInRoles(environmentAgents, roles).ToArray();

            //PrintFoundAgents(roleAgents);
            return CreateCollection(roleAgents);
        }

        public IAgentConnection ConnectTo(ServerAgent agent)
        {
            var connection = connectionClient.RequestConnection(agent);

            //context.agentConnections.Add(connection);
            return connection;
        }

        private IEnumerable<ServerAgent> GetAllAgents()
        {
            if (Agents?.Any() ?? false) return Agents;

            //context.Output.WriteLine("No agents have been defined!", ConsoleColor.DarkRed);
            throw new ApplicationException("No agents have been defined!");
        }

        private IEnumerable<ServerAgent> AgentsInRoles(IEnumerable<ServerAgent> agents, params string[] roles)
        {
            var roleAgents = agents.Where(a => a.MatchesRoles(roles)).ToArray();

            if (roleAgents.Any()) return roleAgents;

            //context.Output.Write("No agents were found in roles ", ConsoleColor.DarkYellow);

            //var i = 0;
            //foreach (var role in roles) {
            //    if (i > 0) context.Output.Write(", ", ConsoleColor.DarkYellow);
            //    i++;

            //    context.Output.Write(role, ConsoleColor.Yellow);
            //}

            //context.Output.WriteLine("!", ConsoleColor.DarkYellow);

            throw new ApplicationException($"No agents were found in roles '{string.Join(", ", roles)}'!");
        }

        private IEnumerable<ServerAgent> AgentsInEnvironment(IEnumerable<ServerAgent> agents, string environmentName)
        {
            var environmentList = Project?.Environments;

            if (!(environmentList?.Any() ?? false)) {
                //context.Output.WriteLine("No environments have been defined!", ConsoleColor.DarkRed);
                throw new ApplicationException("No environments have been defined!");
            }

            var environment = environmentList.FirstOrDefault(x =>
                string.Equals(x.Name, environmentName, StringComparison.OrdinalIgnoreCase));

            if (environment == null) {
                //context.Output.WriteLine($"Environment '{environmentName}' was not found!", ConsoleColor.DarkYellow);
                throw new ApplicationException($"Environment '{environmentName}' was not found!");
            }

            var environmentAgents = agents
                .Where(a => environment.AgentIdList.Contains(a.Id, StringComparer.OrdinalIgnoreCase))
                .ToArray();

            if (environmentAgents.Any())
                return environmentAgents;

            //context.Output.WriteLine($"No agents were found in environment '{environmentName}'!", ConsoleColor.DarkYellow);
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

        private WorkerAgentConnectionCollection CreateCollection(IEnumerable<ServerAgent> agents)
        {
            var connections = agents.Select(ConnectTo).ToArray();
            //context.agentConnections.Add(handle);

            return new WorkerAgentConnectionCollection(connections);
        }

        //private void PrintFoundAgents(IEnumerable<ServerAgent> agents)
        //{
        //    var agentNames = agents.Select(x => x.Name);
        //    context.Output.Write("Found Agents: ", ConsoleColor.DarkCyan);

        //    var i = 0;
        //    foreach (var name in agentNames) {
        //        if (i > 0) context.Output.Write("; ", ConsoleColor.DarkCyan);
        //        i++;

        //        context.Output.Write(name, ConsoleColor.Cyan);
        //    }

        //    context.Output.WriteLine(".", ConsoleColor.DarkCyan);
        //}
    }
}
