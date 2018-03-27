using Photon.Framework.Scripts;
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

        public void RunScript(ScriptContext context)
        {
            registry.ExecuteScript(context);
        }
    }
}
