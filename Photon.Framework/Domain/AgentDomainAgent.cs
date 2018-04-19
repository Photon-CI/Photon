using Photon.Framework.Agent;
using Photon.Framework.Extensions;
using Photon.Framework.Tasks;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Framework.Domain
{
    public class AgentDomainAgent : DomainAgentBase
    {
        private readonly BuildTaskRegistry buildTaskRegistry;
        private readonly DeployTaskRegistry deployTaskRegistry;


        public AgentDomainAgent()
        {
            buildTaskRegistry = new BuildTaskRegistry();
            deployTaskRegistry = new DeployTaskRegistry();
        }

        protected override void OnAssemblyLoaded(Assembly assembly)
        {
            base.OnAssemblyLoaded(assembly);

            buildTaskRegistry.ScanAssembly(assembly);
            deployTaskRegistry.ScanAssembly(assembly);
        }

        public string[] GetBuildTasks()
        {
            return buildTaskRegistry.AllNames.ToArray();
        }

        public string[] GetDeployTasks()
        {
            return deployTaskRegistry.AllNames.ToArray();
        }

        public void RunBuildTask(IAgentBuildContext context, RemoteTaskCompletionSource<object> completeEvent)
        {
            Task.Run(async () => {
                await buildTaskRegistry.ExecuteTask(context, CancellationToken.None);
                return (object) null;
            }).ContinueWith(completeEvent.FromTask);
        }

        public void RunDeployTask(IAgentDeployContext context, RemoteTaskCompletionSource<object> completeEvent)
        {
            Task.Run(async () => {
                await deployTaskRegistry.ExecuteTask(context, CancellationToken.None);
                return (object) null;
            }).ContinueWith(completeEvent.FromTask);
        }
    }
}
