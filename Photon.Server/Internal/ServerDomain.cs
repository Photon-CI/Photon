using Photon.Framework.Domain;
using Photon.Library.Session;

namespace Photon.Server.Internal
{
    internal class ServerDomain : SessionDomainBase<ServerDomainAgent>
    {
        public void RunScript(string scriptName, string jsonData = null)
        {
            agent.RunScript(scriptName, jsonData);
        }
    }
}
