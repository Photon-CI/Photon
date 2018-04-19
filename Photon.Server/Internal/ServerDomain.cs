using Photon.Framework.Domain;
using Photon.Framework.Server;
using Photon.Library.Session;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Server.Internal
{
    internal class ServerDomain : SessionDomainBase<ServerDomainAgent>
    {
        public string[] GetDeployScripts()
        {
            return Agent.GetDeployScripts();
        }

        public async Task RunDeployScript(IServerDeployContext context, CancellationToken token)
        {
            var completeEvent = new RemoteTaskCompletionSource(token);
            Agent.RunDeployScript(context, completeEvent);
            await completeEvent.Task;
        }
    }
}
