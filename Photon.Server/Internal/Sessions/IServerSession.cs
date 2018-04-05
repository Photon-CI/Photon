using Photon.Library;
using System;
using System.Threading.Tasks;

namespace Photon.Server.Internal.Sessions
{
    internal interface IServerSession : IReferenceItem, IDisposable
    {
        Exception Exception {get; set;}

        Task RunAsync();
        Task ReleaseAsync();
        void PrepareWorkDirectory();
    }
}
