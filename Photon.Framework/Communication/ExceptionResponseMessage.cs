namespace Photon.Framework.Communication
{
    public class ExceptionResponseMessage : IResponseMessage
    {
        public string MessageId {get; set;}
        public string RequestMessageId {get; set;}
        public string Exception {get; set;}
    }
}
