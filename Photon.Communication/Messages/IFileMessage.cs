namespace Photon.Communication.Messages
{
    public interface IFileMessage : IMessage
    {
        string Filename {get; set;}
    }
}
