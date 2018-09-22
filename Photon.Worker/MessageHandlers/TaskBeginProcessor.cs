using Photon.Communication;
using Photon.Communication.Messages;
using Photon.Library.TcpMessages.Worker;
using Photon.Worker.Internal;
using System.Threading.Tasks;

namespace Photon.Worker.MessageHandlers
{
    internal class TaskBeginProcessor : MessageProcessorBase<WorkerTaskBeginRequestMessage>
    {
        public override Task<IResponseMessage> Process(WorkerTaskBeginRequestMessage requestMessage)
        {
            var context = (MessageContext)Transceiver.Context;

            //...

            var response = new WorkerTaskBeginResponseMessage();
            return Task.FromResult<IResponseMessage>(response);
        }
    }
}
