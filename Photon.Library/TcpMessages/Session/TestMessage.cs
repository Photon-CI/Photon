using Photon.Communication.Messages;

namespace Photon.Library.TcpMessages.Worker
{
    public class TestMessageRequest : IRequestMessage
    {
        public string MessageId {get; set;}
        public string Text {get; set;}
    }

    public class TestMessageResponse : ResponseMessageBase
    {
        public string Text {get; set;}
    }
}
