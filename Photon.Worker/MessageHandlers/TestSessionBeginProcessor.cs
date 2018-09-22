using Photon.Communication;
using Photon.Communication.Messages;
using Photon.Library.TcpMessages.Session;
using Photon.Worker.Internal;
using Photon.Worker.Internal.Session;
using System.Threading.Tasks;

namespace Photon.Worker.MessageHandlers
{
    internal class TestSessionBeginProcessor : MessageProcessorBase<WorkerTestSessionBeginRequest>
    {
        public override Task<IResponseMessage> Process(WorkerTestSessionBeginRequest requestMessage)
        {
            var context = this.GetMessageContext();

            context.Session = new WorkerTestSession();
            context.Begin();

            var response = new ResponseMessageBase();
            return Task.FromResult<IResponseMessage>(response);
        }
    }
}
