using System.Threading;
using System.Threading.Tasks;

namespace Photon.Framework.AgentConnection
{
    public interface IWorkerAgentConnectionCollection
    {
        Task RunTasksAsync(string[] taskNames, CancellationToken token = default);
        Task ReleaseAsync(CancellationToken token = default);
    }
}
