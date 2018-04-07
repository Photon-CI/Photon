using log4net;
using Photon.Communication;
using Photon.Framework.Messages;
using Photon.Server.Internal;
using System.Threading.Tasks;

namespace Photon.Server.MessageProcessors
{
    internal class SessionOutputProcessor : MessageProcessorBase<AgentSessionOutputMessage>
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SessionOutputProcessor));


        public override async Task<IResponseMessage> Process(AgentSessionOutputMessage requestMessage)
        {
            if (!PhotonServer.Instance.Sessions.TryGetSession(requestMessage.ServerSessionId, out var session)) {
                Log.Warn($"Unable to map response, Server Session '{requestMessage.ServerSessionId}' not found!");
                return null;
            }

            session.Output.AppendRaw(requestMessage.Text);

            return null;
        }
    }
}
