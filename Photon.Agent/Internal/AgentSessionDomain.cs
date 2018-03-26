using Photon.Framework.Domain;
using Photon.Library.Session;

namespace Photon.Agent.Internal
{
    internal class AgentSessionDomain : SessionDomainBase<AgentDomainAgent>
    {
        public void RunTask(string taskName, string jsonData = null)
        {
            agent.RunTask(taskName, jsonData);
        }
    }
}
