using Photon.Framework.Agent;
using Photon.Framework.Domain;
using Photon.Library.Session;
using System.Threading;
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

        public async Task RunBuildTask(AgentBuildContext context, CancellationToken token)
        {
            Sponsor.Register(context.Output);
            Sponsor.Register(context.Packages);

            try {
                var completeEvent = new RemoteTaskCompletionSource(token);
                Agent.RunBuildTask(context, completeEvent);
                await completeEvent.Task;
            }
            finally {
                Sponsor.Unregister(context.Packages);
                Sponsor.Unregister(context.Output);
            }
        }

        public async Task RunDeployTask(AgentDeployContext context, CancellationToken token)
        {
            Sponsor.Register(context.Output);
            Sponsor.Register(context.Packages);

            try {
                var completeEvent = new RemoteTaskCompletionSource(token);
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
