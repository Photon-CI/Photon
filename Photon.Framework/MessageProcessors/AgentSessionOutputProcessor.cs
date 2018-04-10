using Photon.Communication;
using Photon.Communication.Messages;
using Photon.Framework.Scripts;
using Photon.Framework.Sessions;
using Photon.Framework.TcpMessages;
using System;
using System.Threading.Tasks;

namespace Photon.Framework.MessageProcessors
{
    internal class AgentSessionOutputProcessor : MessageProcessorBase<AgentSessionOutputMessage>
    {
        public override async Task<IResponseMessage> Process(AgentSessionOutputMessage requestMessage)
        {
            if (Transceiver.Context is IServerDeployContext deployContext) {
                deployContext.Output.AppendRaw(requestMessage.Text);
                return null;
            }

            if (Transceiver.Context is IServerSession session) {
                session.Output.AppendRaw(requestMessage.Text);
                return null;
            }

            throw new Exception("Server Context is undefined!");
        }
    }
}
