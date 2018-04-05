using System.Threading.Tasks;

namespace Photon.Framework.Tasks
{
    public interface IBuildTask
    {
        Task<TaskResult> RunAsync(IAgentBuildContext context);
    }
}
