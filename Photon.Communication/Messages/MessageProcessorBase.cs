namespace Photon.Communication.Messages
{
    public abstract class MessageProcessorBase : IProcessMessage
    {
        public MessageTransceiver Transceiver {get; set;}
    }
}
