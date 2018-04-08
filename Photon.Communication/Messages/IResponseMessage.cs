namespace Photon.Communication.Messages
{
    public interface IResponseMessage : IMessage
    {
        string RequestMessageId {get; set;}
    }
}
