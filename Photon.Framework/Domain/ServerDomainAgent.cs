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

        public override Assembly LoadAssembly(string filename)
        {
            var assembly = base.LoadAssembly(filename);

            registry.ScanAssembly(assembly);

            return assembly;
        }

        public void RunScript(ScriptContext context)
        {
            registry.ExecuteScript(context);
        }
    }
}
