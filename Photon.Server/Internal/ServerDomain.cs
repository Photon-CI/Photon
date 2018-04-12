using Photon.Framework.Domain;
using Photon.Framework.Server;
using Photon.Framework.Tasks;
using Photon.Library.Session;
using System.Threading.Tasks;

namespace Photon.Server.Internal
{
    internal class ServerDomain : SessionDomainBase<ServerDomainAgent>
    {
        public string[] GetDeployScripts()
        {
            return Agent.GetDeployScripts();
        }

        public async Task<TaskResult> RunDeployScript(IServerDeployContext context)
        {
            var completeEvent = new RemoteTaskCompletionSource<TaskResult>();
            Agent.RunDeployScript(context, completeEvent);
            return await completeEvent.Task;
        }
    }
}
