using Photon.Framework.Pooling;
using Photon.Framework.Server;
using Photon.Framework.Tasks;
using System;
using System.Threading.Tasks;

namespace Photon.Server.Internal.Sessions
{
    public interface IServerSession : IReferenceItem, IDisposable
    {
        string WorkDirectory {get;}
        string BinDirectory {get;}
        string ContentDirectory {get;}
        Exception Exception {get; set;}
        ScriptOutput Output {get;}
        TaskResult Result {get;}
        bool IsComplete {get;}

        //DomainAgentSessionHostBase GetAgentSession(string agentSessionId);
        Task PrepareWorkDirectoryAsync();
        Task<TaskResult> RunAsync();
        Task ReleaseAsync();
        void Complete(TaskResult result);
    }
}
