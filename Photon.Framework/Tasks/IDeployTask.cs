using System.Threading.Tasks;

namespace Photon.Framework.Tasks
{
    public interface IDeployTask
    {
        Task<TaskResult> RunAsync(IDeployTaskContext context);
    }
}
