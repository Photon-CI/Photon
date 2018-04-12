using Photon.Framework.Pooling;
using Photon.Framework.Server;
using System;
using System.Threading.Tasks;

namespace Photon.Framework.Sessions
{
    public interface IServerSession : IReferenceItem, IDisposable
    {
        string WorkDirectory {get;}
        string BinDirectory {get;}
        string ContentDirectory {get;}
        Exception Exception {get; set;}
        ScriptOutput Output {get;}
        bool IsComplete {get;}

        Task RunAsync();
        Task ReleaseAsync();
        Task PrepareWorkDirectoryAsync();
        void Complete();
    }
}
