using Photon.Library;
using System;
using System.Threading.Tasks;
using Photon.Framework.Scripts;

namespace Photon.Server.Internal.Sessions
{
    internal interface IServerSession : IReferenceItem, IDisposable
    {
        Exception Exception {get; set;}
        ScriptOutput Output {get;}
        bool Complete {get;}

        Task RunAsync();
        Task ReleaseAsync();
        void PrepareWorkDirectory();
    }
}
