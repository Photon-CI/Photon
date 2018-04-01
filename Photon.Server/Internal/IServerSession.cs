using Photon.Library;
using System;
using System.Threading.Tasks;

namespace Photon.Server.Internal
{
    internal interface IServerSession : IReferenceItem, IDisposable
    {
        Exception Exception {get; set;}

        Task RunAsync();
        Task ReleaseAsync();
        void PrepareWorkDirectory();
    }
}
