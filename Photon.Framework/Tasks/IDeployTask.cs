using Photon.Framework.Agent;
using System.Threading.Tasks;

namespace Photon.Framework.Tasks
{
    public interface IDeployTask
    {
        IAgentDeployContext Context {get; set;}

        Task<TaskResult> RunAsync();
    }
}
