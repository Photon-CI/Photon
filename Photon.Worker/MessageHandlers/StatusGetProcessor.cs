using Photon.Communication;
using Photon.Communication.Messages;
using Photon.Library.TcpMessages.Status;
using Photon.Worker.Internal;
using System.Threading.Tasks;

namespace Photon.Worker.MessageHandlers
{
    internal class StatusGetProcessor : MessageProcessorBase<WorkerStatusGetRequest>
    {
        public override Task<IResponseMessage> Process(WorkerStatusGetRequest requestMessage)
        {
            var context = this.GetMessageContext();

            var response = new WorkerStatusGetResponse {
                HostName = "?",
            };

            return Task.FromResult<IResponseMessage>(response);
        }
    }
}
