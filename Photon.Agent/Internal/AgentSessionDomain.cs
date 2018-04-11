using Photon.Framework.Agent;
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
            return Agent.GetBuildTasks();
        }

        public string[] GetDeployTasks()
        {
            return Agent.GetDeployTasks();
        }

        public async Task<TaskResult> RunBuildTask(AgentBuildContext context)
        {
            //Sponsor.Register(context);
            Sponsor.Register(context.Output);

            TaskResult result;
            try {
                var completeEvent = new RemoteTaskCompletionSource<TaskResult>();
                Agent.RunBuildTask(context, completeEvent);
                result = await completeEvent.Task;
            }
            finally {
                Sponsor.Unregister(context.Output);
                //Sponsor.Unregister(context);
            }

            return result;
        }

        public async Task<TaskResult> RunDeployTask(AgentDeployContext context)
        {
            //Sponsor.Register(context);
            Sponsor.Register(context.Output);

            TaskResult result;
            try {
                var completeEvent = new RemoteTaskCompletionSource<TaskResult>();
                Agent.RunDeployTask(context, completeEvent);
                result = await completeEvent.Task;
            }
            finally {
                Sponsor.Unregister(context.Output);
                //Sponsor.Unregister(context);
            }

            return result;
        }
    }
}
