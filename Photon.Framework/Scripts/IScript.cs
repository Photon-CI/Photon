using System.Threading.Tasks;

namespace Photon.Framework.Scripts
{
    public interface IScript
    {
        Task<ScriptResult> RunAsync(ScriptContext context);
    }
}
