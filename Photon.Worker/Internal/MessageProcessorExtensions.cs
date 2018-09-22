using Photon.Communication;
using Photon.Communication.Messages;

namespace Photon.Worker.Internal
{
    internal static class MessageProcessorExtensions
    {
        public static MessageContext GetMessageContext<T>(this MessageProcessorBase<T> messageProcessor)
            where T : IRequestMessage
        {
            return (MessageContext) messageProcessor.Transceiver.Context;
        }
    }
}
