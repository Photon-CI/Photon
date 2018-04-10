using Photon.Framework;
using Photon.Framework.Tasks;
using System;
using System.Threading.Tasks;

namespace Photon.Agent.Internal.Session
{
    internal interface IAgentSession : IReferenceItem, IDisposable
    {
        Exception Exception {get; set;}

        Task InitializeAsync();
        Task<TaskResult> RunTaskAsync(string taskName, string taskSessionId);
        //SessionTaskHandle BeginTask(string taskName);
        Task ReleaseAsync();
    }
}
