using Photon.Framework.Domain;
using Photon.Framework.Tasks;
using Photon.Library.Session;
using System.Threading.Tasks;

namespace Photon.Agent.Internal
{
    internal class AgentSessionDomain : SessionDomainBase<AgentDomainAgent>
    {
        public string[] GetTasks()
        {
            return agent.GetTasks();
        }

        public async Task<TaskResult> RunTask(TaskContext context)
        {
            var completeEvent = new RemoteTaskCompletionSource<TaskResult>();
            agent.RunTask(context, completeEvent);
            return await completeEvent.Task;
        }
    }
}
