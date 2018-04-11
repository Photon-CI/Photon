using Photon.Communication;
using Photon.Framework.Pooling;
using Photon.Framework.Tasks;
using System;
using System.Threading.Tasks;

namespace Photon.Agent.Internal.Session
{
    internal interface IAgentSession : IReferenceItem, IDisposable
    {
        MessageTransceiver Transceiver {get;}
        Exception Exception {get; set;}

        Task InitializeAsync();
        Task ReleaseAsync();
        Task<TaskResult> RunTaskAsync(string taskName, string taskSessionId);
    }
}
