using Photon.Framework.Extensions;
using Photon.Framework.Scripts;
using System.Linq;
using System.Reflection;

namespace Photon.Framework.Domain
{
    public class ServerDomainAgent : DomainAgentBase
    {
        private readonly BuildScriptRegistry buildScriptRegistry;
        private readonly DeployScriptRegistry deployScriptRegistry;


        public ServerDomainAgent()
        {
            buildScriptRegistry = new BuildScriptRegistry();
            deployScriptRegistry = new DeployScriptRegistry();
        }

        protected override void OnAssemblyLoaded(Assembly assembly)
        {
            base.OnAssemblyLoaded(assembly);

            buildScriptRegistry.ScanAssembly(assembly);
            deployScriptRegistry.ScanAssembly(assembly);
        }

        public string[] GetBuildScripts()
        {
            return buildScriptRegistry.AllNames.ToArray();
        }

        public string[] GetDeployScripts()
        {
            return deployScriptRegistry.AllNames.ToArray();
        }

        public void RunBuildScript(IServerBuildContext context, RemoteTaskCompletionSource<ScriptResult> completeEvent)
        {
            buildScriptRegistry.ExecuteScript(context)
                .ContinueWith(completeEvent.FromTask);
        }

        public void RunDeployScript(IServerDeployContext context, RemoteTaskCompletionSource<ScriptResult> completeEvent)
        {
            deployScriptRegistry.ExecuteScript(context)
                .ContinueWith(completeEvent.FromTask);
        }
    }
}
