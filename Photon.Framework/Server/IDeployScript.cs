using System.Threading.Tasks;

namespace Photon.Framework.Server
{
    public interface IDeployScript
    {
        Task<ScriptResult> RunAsync(IServerDeployContext context);
    }
}
