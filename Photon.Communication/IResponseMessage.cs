namespace Photon.Communication
{
    public interface IResponseMessage : IMessage
    {
        string RequestMessageId {get; set;}
    }
}
