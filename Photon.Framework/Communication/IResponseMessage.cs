namespace Photon.Framework.Communication
{
    public interface IResponseMessage : IMessage
    {
        string RequestMessageId {get; set;}
    }
}
