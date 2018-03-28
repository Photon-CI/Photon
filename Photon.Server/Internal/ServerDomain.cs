using Photon.Framework.Domain;
using Photon.Framework.Scripts;
using Photon.Library.Session;

namespace Photon.Server.Internal
{
    internal class ServerDomain : SessionDomainBase<ServerDomainAgent>
    {
        public string[] GetScripts()
        {
            return agent.GetScripts();
        }

        public void RunScript(ScriptContext context)
        {
            agent.RunScript(context);
        }
    }
}
