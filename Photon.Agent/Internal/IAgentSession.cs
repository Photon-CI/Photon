using Photon.Library;
using System;
using System.Threading.Tasks;

namespace Photon.Agent.Internal
{
    internal interface IAgentSession : IReferenceItem, IDisposable
    {
        Exception Exception {get; set;}

        Task RunTaskAsync(string taskName);
        Task ReleaseAsync();
        //void PrepareWorkDirectory();
    }
}
