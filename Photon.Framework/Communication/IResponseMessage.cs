namespace Photon.Framework.Communication
{
    public interface IResponseMessage
    {
        string RequestMessageId {get;}
        string MessageId {get;}
    }
}
