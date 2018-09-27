using Photon.Communication;
using Photon.Framework.AgentConnection;
using Photon.Framework.Projects;
using Photon.Framework.Server;
using Photon.Library.TcpMessages.AgentConnection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Photon.Worker.Internal.Clients
{
    internal class AgentConnectionClient : IAgentConnectionClient
    {
        private readonly MessageTransceiver transceiver;

        public ServerAgent[] Agents {get; set;}
        public ProjectEnvironment[] Environments {get; set;}


        public AgentConnectionClient(MessageTransceiver transceiver)
        {
            this.transceiver = transceiver;
        }

        public Task<IWorkerAgentConnectionCollection> Any(params string[] roles)
        {
            var agents = GetAllAgents();
            var roleAgents = AgentsInRoles(agents, roles);
            var randomAgent = GetRandomAgent(roleAgents);

            return ConnectToAgents(randomAgent);
        }

        public Task<IWorkerAgentConnectionCollection> All()
        {
            var agents = GetAllAgents().ToArray();

            return ConnectToAgents(agents);
        }

        public Task<IWorkerAgentConnectionCollection> All(params string[] roles)
        {
            var agents = GetAllAgents();
            var roleAgents = AgentsInRoles(agents, roles).ToArray();

            return ConnectToAgents(roleAgents);
        }

        public Task<IWorkerAgentConnectionCollection> Environment(string name)
        {
            var agents = GetAllAgents();
            var environmentAgents = AgentsInEnvironment(agents, name).ToArray();

            return ConnectToAgents(environmentAgents);
        }

        public Task<IWorkerAgentConnectionCollection> Environment(string name, params string[] roles)
        {
            var allAgents = GetAllAgents();
            var environmentAgents = AgentsInEnvironment(allAgents, name);
            var roleAgents = AgentsInRoles(environmentAgents, roles).ToArray();

            return ConnectToAgents(roleAgents);
        }

        private async Task<IWorkerAgentConnectionCollection> ConnectToAgents(params ServerAgent[] agents)
        {
            var request = new WorkerAgentRequestRequest {
                AgentIdList = agents.Select(x => x.Id).ToArray(),
            };

            var response = await transceiver.Send(request)
                .GetResponseAsync<WorkerAgentRequestResponse>();

            var connectionHandles = response.ConnectionHandles
                .Select(x => new WorkerAgentConnection {
                    AgentId = x.AgentId,
                    ConnectionId = x.ConnectionId,
                    Transceiver = transceiver,
                }).ToArray();

            return new WorkerAgentConnectionCollection(connectionHandles);
        }

        private IEnumerable<ServerAgent> GetAllAgents()
        {
            if (Agents?.Any() ?? false) return Agents;

            throw new ApplicationException("No agents have been defined!");
        }

        private IEnumerable<ServerAgent> AgentsInRoles(IEnumerable<ServerAgent> agents, params string[] roles)
        {
            var roleAgents = agents.Where(a => a.MatchesRoles(roles)).ToArray();

            if (roleAgents.Any()) return roleAgents;

            throw new ApplicationException($"No agents were found in roles '{string.Join(", ", roles)}'!");
        }

        private IEnumerable<ServerAgent> AgentsInEnvironment(IEnumerable<ServerAgent> agents, string environmentName)
        {
            if (!(Environments?.Any() ?? false))
                throw new ApplicationException("No environments have been defined!");

            var environment = Environments.FirstOrDefault(x =>
                string.Equals(x.Name, environmentName, StringComparison.OrdinalIgnoreCase));

            if (environment == null)
                throw new ApplicationException($"Environment '{environmentName}' was not found!");

            var environmentAgents = agents
                .Where(a => environment.AgentIdList.Contains(a.Id, StringComparer.OrdinalIgnoreCase))
                .ToArray();

            if (environmentAgents.Any())
                return environmentAgents;

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
    }
}
