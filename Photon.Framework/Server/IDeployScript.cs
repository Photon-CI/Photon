using System.Threading;
using System.Threading.Tasks;

namespace Photon.Framework.Server
{
    public interface IDeployScript
    {
        IServerDeployContext Context {get; set;}
        Task RunAsync(CancellationToken token);
    }
}
