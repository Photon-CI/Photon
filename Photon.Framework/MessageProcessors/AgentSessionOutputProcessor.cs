using Photon.Communication;
using Photon.Communication.Messages;
using Photon.Framework.Server;
using Photon.Framework.TcpMessages;
using System;
using System.Threading.Tasks;

namespace Photon.Framework.MessageProcessors
{
    internal class AgentSessionOutputProcessor : MessageProcessorBase<AgentSessionOutputMessage>
    {
        public override async Task<IResponseMessage> Process(AgentSessionOutputMessage requestMessage)
        {
            if (!(Transceiver.Context is IServerContext deployContext))
                throw new Exception("Server Context is undefined!");

            deployContext.Output.AppendRaw(requestMessage.Text);
            return null;
        }
    }
}
