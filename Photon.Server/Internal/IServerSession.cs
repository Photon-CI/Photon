using Photon.Library;
using System;

namespace Photon.Server.Internal
{
    internal interface IServerSession : IReferenceItem, IDisposable
    {
        Exception Exception {get; set;}

        void Run();
        void Release();
        void PrepareWorkDirectory();
    }
}
