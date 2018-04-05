using System.Threading.Tasks;

namespace Photon.Framework.Scripts
{
    public interface IDeployScript
    {
        Task<ScriptResult> RunAsync(IServerDeployContext context);
    }
}
