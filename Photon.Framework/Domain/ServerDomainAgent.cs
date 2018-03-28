using Photon.Framework.Scripts;
using System.Linq;
using System.Reflection;

namespace Photon.Framework.Domain
{
    public class ServerDomainAgent : DomainAgentBase
    {
        private readonly ScriptRegistry registry;


        public ServerDomainAgent()
        {
            registry = new ScriptRegistry();
        }

        protected override void OnAssemblyLoaded(Assembly assembly)
        {
            base.OnAssemblyLoaded(assembly);

            registry.ScanAssembly(assembly);
        }

        public string[] GetScripts()
        {
            return registry.AllNames.ToArray();
        }

        public void RunScript(ScriptContext context, RemoteTaskCompletionSource<ScriptResult> completeEvent)
        {
            registry.ExecuteScript(context)
                .ContinueWith(t => {
                    if (t.IsCanceled) completeEvent.SetCancelled();
                    else if (t.IsFaulted) completeEvent.SetException(t.Exception);
                    else completeEvent.SetResult(t.Result);
                });
        }
    }
}
