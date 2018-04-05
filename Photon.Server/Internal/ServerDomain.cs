using Photon.Framework.Domain;
using Photon.Framework.Scripts;
using Photon.Library.Session;
using Photon.Server.Internal.Scripts;
using System.Threading.Tasks;

namespace Photon.Server.Internal
{
    internal class ServerDomain : SessionDomainBase<ServerDomainAgent>
    {
        public string[] GetBuildScripts()
        {
            return agent.GetBuildScripts();
        }

        public string[] GetDeployScripts()
        {
            return agent.GetDeployScripts();
        }

        public async Task<ScriptResult> RunBuildScript(ServerBuildContext context)
        {
            var completeEvent = new RemoteTaskCompletionSource<ScriptResult>();
            agent.RunBuildScript(context, completeEvent);
            return await completeEvent.Task;
        }

        public async Task<ScriptResult> RunDeployScript(ServerDeployContext context)
        {
            var completeEvent = new RemoteTaskCompletionSource<ScriptResult>();
            agent.RunDeployScript(context, completeEvent);
            return await completeEvent.Task;
        }
    }
}
