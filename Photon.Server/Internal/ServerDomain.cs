using Photon.Framework.Domain;
using Photon.Framework.Scripts;
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

        public async Task<ScriptResult> RunDeployScript(IServerDeployContext context)
        {
            var completeEvent = new RemoteTaskCompletionSource<ScriptResult>();
            Agent.RunDeployScript(context, completeEvent);
            return await completeEvent.Task;
        }
    }
}
