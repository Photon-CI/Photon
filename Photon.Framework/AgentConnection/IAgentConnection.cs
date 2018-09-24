using System;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Framework.AgentConnection
{
    public interface IAgentConnection : IDisposable
    {
        Task BeginAsync(CancellationToken token);
        Task ReleaseAsync(CancellationToken token);
        Task RunTaskAsync(string taskName, CancellationToken token = default);
    }
}
