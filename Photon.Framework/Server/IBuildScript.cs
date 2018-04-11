using System.Threading.Tasks;

namespace Photon.Framework.Scripts
{
    public interface IBuildScript
    {
        Task<ScriptResult> RunAsync(IServerBuildContext context);
    }
}
