using Photon.Communication;
using Photon.Communication.Messages;
using Photon.Library.TcpMessages;
using Photon.Worker.Internal;
using System.Threading.Tasks;

namespace Photon.Worker.MessageHandlers
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
