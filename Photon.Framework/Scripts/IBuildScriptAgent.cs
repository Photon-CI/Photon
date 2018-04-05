using System.Threading.Tasks;

namespace Photon.Framework.Scripts
{
    public interface IBuildScriptAgent
    {
        Task<IAgentBuildSession> BeginSession();
    }
}
