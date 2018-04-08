using System.Threading.Tasks;

namespace Photon.Communication.Messages
{
    public interface IProcessMessage<in TRequest>
        where TRequest : IRequestMessage
    {
        MessageTransceiver Transceiver {get; set;}

        Task<IResponseMessage> Process(TRequest requestMessage);
    }
}
