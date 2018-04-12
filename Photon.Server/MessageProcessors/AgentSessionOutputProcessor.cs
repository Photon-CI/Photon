using Photon.Communication;
using Photon.Communication.Messages;
using Photon.Library.TcpMessages;
using Photon.Server.Internal;
using System;
using System.Threading.Tasks;

namespace Photon.Server.MessageProcessors
{
    internal class AgentSessionOutputProcessor : MessageProcessorBase<SessionOutputMessage>
    {
        public override async Task<IResponseMessage> Process(SessionOutputMessage requestMessage)
        {
            if (!PhotonServer.Instance.Sessions.TryGet(requestMessage.ServerSessionId, out var session))
                throw new Exception($"Agent Session ID '{requestMessage.ServerSessionId}' not found!");

            await Task.Run(() => session.Output.AppendRaw(requestMessage.Text));
            return null;
        }
    }
}
