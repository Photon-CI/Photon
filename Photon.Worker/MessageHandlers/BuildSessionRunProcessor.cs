using Photon.Communication;
using Photon.Communication.Messages;
using Photon.Library.TcpMessages.Session;
using Photon.Worker.Internal;
using System.Threading.Tasks;

namespace Photon.Worker.MessageHandlers
{
    internal class BuildSessionRunProcessor : MessageProcessorBase<WorkerBuildSessionRunRequest>
    {
        //private static readonly ILog Log = LogManager.GetLogger();

        public override async Task<IResponseMessage> Process(WorkerBuildSessionRunRequest requestMessage)
        {
            var context = this.GetMessageContext();

            try {
                // TODO Log

                await context.Run();

                // TODO Log
            }
            catch {
                // TODO Log
                throw;
            }

            return new ResponseMessageBase();
        }
    }
}
