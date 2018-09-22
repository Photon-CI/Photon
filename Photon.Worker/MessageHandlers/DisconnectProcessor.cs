using Photon.Communication;
using Photon.Communication.Messages;
using Photon.Library.TcpMessages.Worker;
using Photon.Worker.Internal;
using System.Threading.Tasks;

namespace Photon.Worker.MessageHandlers
{
    internal class DisconnectProcessor : MessageProcessorBase<WorkerDisconnectRequestMessage>
    {
        public override Task<IResponseMessage> Process(WorkerDisconnectRequestMessage requestMessage)
        {
            var context = (MessageContext)Transceiver.Context;
            context.CompleteTask.SetResult(null);

            var response = new WorkerDisconnectResponseMessage();
            return Task.FromResult<IResponseMessage>(response);
        }
    }
}
