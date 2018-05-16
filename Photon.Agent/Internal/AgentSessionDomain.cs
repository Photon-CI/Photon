using Photon.Framework.Agent;
using Photon.Framework.Domain;
using Photon.Library.Session;
using System.Threading;
using System.Threading.Tasks;
using Photon.Framework.Tasks;

namespace Photon.Agent.Internal
{
    internal class AgentSessionDomain : SessionDomainBase<AgentDomainAgent>
    {
        public TaskDescription[] GetBuildTasks()
        {
            return Agent.GetBuildTasks();
        }

        public TaskDescription[] GetDeployTasks()
        {
            return Agent.GetDeployTasks();
        }

        public async Task RunBuildTask(AgentBuildContext context, CancellationToken token = default(CancellationToken))
        {
            var completeEvent = new RemoteTaskCompletionSource();
            token.Register(completeEvent.SetCancelled);

            Sponsor.Register(context.Output);
            Sponsor.Register(context.Packages);

            try {
                Agent.RunBuildTask(context, completeEvent);
                await completeEvent.Task;
            }
            finally {
                Sponsor.Unregister(context.Packages);
                Sponsor.Unregister(context.Output);
            }
        }

        public async Task RunDeployTask(AgentDeployContext context, CancellationToken token = default(CancellationToken))
        {
            var completeEvent = new RemoteTaskCompletionSource();
            token.Register(completeEvent.SetCancelled);

            Sponsor.Register(context.Output);
            Sponsor.Register(context.Packages);

            try {
                Agent.RunDeployTask(context, completeEvent);
                await completeEvent.Task;
            }
            finally {
                Sponsor.Unregister(context.Packages);
                Sponsor.Unregister(context.Output);
            }
        }
    }
}
