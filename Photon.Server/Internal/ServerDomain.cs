using Photon.Framework.Domain;
using Photon.Framework.Scripts;
using Photon.Library.Session;
using System.Threading.Tasks;

namespace Photon.Server.Internal
{
    internal class ServerDomain : SessionDomainBase<ServerDomainAgent>
    {
        public string[] GetScripts()
        {
            return agent.GetScripts();
        }

        public async Task<ScriptResult> RunScript(ScriptContext context)
        {
            var completeEvent = new RemoteTaskCompletionSource<ScriptResult>();
            agent.RunScript(context, completeEvent);
            return await completeEvent.Task;
        }
    }
}
