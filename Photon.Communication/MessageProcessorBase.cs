using System.Threading.Tasks;
using Photon.Communication.Messages;

namespace Photon.Communication
{
    public abstract class MessageProcessorBase<T> : IProcessMessage<T>
        where T : IRequestMessage
    {
        public MessageTransceiver Transceiver {get; set;}


        public abstract Task<IResponseMessage> Process(T requestMessage);
    }
}
