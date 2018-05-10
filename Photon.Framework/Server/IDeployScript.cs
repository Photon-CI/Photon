using System.Threading.Tasks;

namespace Photon.Framework.Server
{
    public interface IDeployScript
    {
        Task RunAsync(IServerDeployContext context);
    }
}
