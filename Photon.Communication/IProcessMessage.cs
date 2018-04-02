using System.Threading.Tasks;

namespace Photon.Communication
{
    public interface IProcessMessage<TRequest>
        where TRequest : IRequestMessage
    {
        Task<IResponseMessage> Process(TRequest requestMessage);
    }
}
