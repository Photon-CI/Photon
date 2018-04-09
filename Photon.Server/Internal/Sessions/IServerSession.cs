using Photon.Framework.Scripts;
using Photon.Library;
using System;
using System.Threading.Tasks;

namespace Photon.Server.Internal.Sessions
{
    internal interface IServerSession : IReferenceItem, IDisposable
    {
        string WorkDirectory {get;}
        string BinDirectory {get;}
        string ContentDirectory {get;}
        Exception Exception {get; set;}
        ScriptOutput Output {get;}
        bool Complete {get;}

        Task RunAsync();
        Task ReleaseAsync();
        Task PrepareWorkDirectoryAsync();
    }
}
