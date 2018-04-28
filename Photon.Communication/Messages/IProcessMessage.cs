using System.Threading.Tasks;

namespace Photon.Communication.Messages
{
    public interface IProcessMessage
    {
        MessageTransceiver Transceiver {get; set;}
    }

    public interface IProcessMessage<in TRequest> : IProcessMessage
        where TRequest : IRequestMessage
    {
        Task<IResponseMessage> Process(TRequest requestMessage);
    }
}
