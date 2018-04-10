using log4net;
using Photon.Communication;
using Photon.Server.Internal;
using System.Threading.Tasks;
using Photon.Communication.Messages;
using Photon.Framework.TcpMessages;

namespace Photon.Server.MessageProcessors
{
    internal class AgentSessionOutputProcessor : MessageProcessorBase<AgentSessionOutputMessage>
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(AgentSessionOutputProcessor));


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
