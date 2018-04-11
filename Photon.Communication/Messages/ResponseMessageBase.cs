namespace Photon.Communication.Messages
{
    public class ResponseMessageBase : IResponseMessage
    {
        public string MessageId {get; set;}
        public string RequestMessageId {get; set;}
        public string ExceptionMessage {get; set;}
        public string Exception {get; set;}
        public bool Successful {get; set;}


        public ResponseMessageBase()
        {
            Successful = true;
        }
    }
}
