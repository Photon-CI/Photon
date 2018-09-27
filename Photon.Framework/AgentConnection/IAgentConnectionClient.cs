using System.Threading.Tasks;

namespace Photon.Framework.AgentConnection
{
    public interface IAgentConnectionClient
    {
        Task<IWorkerAgentConnectionCollection> Any(params string[] roles);
        Task<IWorkerAgentConnectionCollection> All();
        Task<IWorkerAgentConnectionCollection> All(params string[] roles);
        Task<IWorkerAgentConnectionCollection> Environment(string name);
        Task<IWorkerAgentConnectionCollection> Environment(string name, params string[] roles);
    }
}
