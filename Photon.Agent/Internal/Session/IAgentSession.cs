using Photon.Communication;
using Photon.Framework.Pooling;
using System;
using System.Threading.Tasks;

namespace Photon.Agent.Internal.Session
{
    internal interface IAgentSession : IReferenceItem, IDisposable
    {
        MessageTransceiver ServerTransceiver {get;}
        Exception Exception {get; set;}

        Task InitializeAsync();
        Task ReleaseAsync();
        //Task RunTaskAsync(string taskName, string taskSessionId);
        //void Cancel(); // TODO: Remove?
        Task AbortAsync();
    }
}
