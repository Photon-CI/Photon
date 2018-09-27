using System;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Framework.AgentConnection
{
    public interface IAgentConnection : IDisposable
    {
        Task RunTaskAsync(string taskName, CancellationToken token = default);
        Task ReleaseAsync(CancellationToken token = default);
    }
}
