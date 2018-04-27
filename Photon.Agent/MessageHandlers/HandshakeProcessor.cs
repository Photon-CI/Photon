using Photon.Agent.Internal;
using Photon.Communication;
using Photon.Communication.Messages;
using Photon.Library.TcpMessages;
using System.Threading.Tasks;

namespace Photon.Agent.MessageHandlers
{
    internal class HandshakeProcessor : MessageProcessorBase<HandshakeRequest>
    {
        public override async Task<IResponseMessage> Process(HandshakeRequest requestMessage)
        {
            var response = new HandshakeResponse {
                Key = requestMessage.Key,
                AgentVersion = Configuration.Version,
                PasswordMatch = true,
            };

            var host = Transceiver.Context as MessageHost;
            host?.CompleteHandshake(response.PasswordMatch);

            return await Task.FromResult(response);
        }
    }
}
