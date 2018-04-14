using Photon.Framework.Tasks;
using System.Threading.Tasks;

namespace Photon.Framework.Server
{
    public interface IDeployScript
    {
        Task<TaskResult> RunAsync(IServerDeployContext context);
    }
}
