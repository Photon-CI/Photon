using Photon.Framework.Extensions;
using Photon.Framework.Server;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Photon.Framework.Domain
{
    public class ServerDomainAgent : DomainAgentBase
    {
        private readonly DeployScriptRegistry deployScriptRegistry;


        public ServerDomainAgent()
        {
            deployScriptRegistry = new DeployScriptRegistry();
        }

        protected override void OnAssemblyLoaded(Assembly assembly)
        {
            base.OnAssemblyLoaded(assembly);

            deployScriptRegistry.ScanAssembly(assembly);
        }

        public string[] GetDeployScripts()
        {
            return deployScriptRegistry.AllNames.ToArray();
        }

        public void RunDeployScript(IServerDeployContext context, RemoteTaskCompletionSource completeEvent)
        {
            deployScriptRegistry.ExecuteScript(context, CancellationToken.None)
                .ContinueWith(completeEvent.FromTask);
        }
    }
}
