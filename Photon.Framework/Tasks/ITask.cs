using System.Threading.Tasks;

namespace Photon.Framework.Tasks
{
    public interface ITask
    {
        Task<TaskResult> RunAsync(TaskContext context);
    }
}
