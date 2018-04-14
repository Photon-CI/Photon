using Photon.Framework.Tasks;
using System;
using System.Threading.Tasks;

namespace Photon.Framework.Server
{
    public interface IAgentSessionHandle : IDisposable
    {
        TaskRunnerManager Tasks {get;}
        string AgentSessionId {get;}

        Task BeginAsync();
        Task ReleaseAsync();
        Task<TaskResult> RunTaskAsync(string taskName);
    }
}
