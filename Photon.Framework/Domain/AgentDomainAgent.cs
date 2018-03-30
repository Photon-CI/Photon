using Photon.Framework.Tasks;
using System.Linq;
using System.Reflection;

namespace Photon.Framework.Domain
{
    public class AgentDomainAgent : DomainAgentBase
    {
        private readonly TaskRegistry registry;


        public AgentDomainAgent()
        {
            registry = new TaskRegistry();
        }

        protected override void OnAssemblyLoaded(Assembly assembly)
        {
            base.OnAssemblyLoaded(assembly);

            registry.ScanAssembly(assembly);
        }

        public string[] GetTasks()
        {
            return registry.AllNames.ToArray();
        }

        public void RunTask(BuildTaskContext context, RemoteTaskCompletionSource<TaskResult> completeEvent)
        {
            registry.ExecuteTask(context)
                .ContinueWith(t => {
                    if (t.IsCanceled) completeEvent.SetCancelled();
                    else if (t.IsFaulted) completeEvent.SetException(t.Exception);
                    else completeEvent.SetResult(t.Result);
                });
        }
    }
}
