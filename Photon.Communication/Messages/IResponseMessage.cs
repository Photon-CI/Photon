namespace Photon.Communication.Messages
{
    public interface IResponseMessage : IMessage
    {
        string RequestMessageId {get; set;}
        string ExceptionMessage {get; set;}
        string Exception {get; set;}
        bool Successful {get; set;}
    }
}
