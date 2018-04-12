using Photon.Communication.Messages;
using System.Threading.Tasks;

namespace Photon.Communication
{
    public abstract class MessageProcessorBase<T> : IProcessMessage<T>
        where T : IRequestMessage
    {
        public MessageTransceiver Transceiver {get; set;}


        public abstract Task<IResponseMessage> Process(T requestMessage);
    }
}
