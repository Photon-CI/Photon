using Photon.Framework.Domain;
using Photon.Framework.Tasks;
using Photon.Library.Session;
using System.Threading.Tasks;

namespace Photon.Agent.Internal
{
    internal class AgentSessionDomain : SessionDomainBase<AgentDomainAgent>
    {
        public string[] GetBuildTasks()
        {
            return agent.GetBuildTasks();
        }

        public string[] GetDeployTasks()
        {
            return agent.GetDeployTasks();
        }

        public async Task<TaskResult> RunBuildTask(AgentBuildContext context)
        {
            var completeEvent = new RemoteTaskCompletionSource<TaskResult>();
            agent.RunBuildTask(context, completeEvent);
            return await completeEvent.Task;
        }

        public async Task<TaskResult> RunDeployTask(AgentDeployContext context)
        {
            var completeEvent = new RemoteTaskCompletionSource<TaskResult>();
            agent.RunDeployTask(context, completeEvent);
            return await completeEvent.Task;
        }
    }
}
