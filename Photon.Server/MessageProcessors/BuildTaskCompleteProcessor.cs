using log4net;
using Photon.Communication;
using Photon.Framework.Tasks;
using Photon.Library.Messages;
using Photon.Server.Internal;
using System.Threading.Tasks;

namespace Photon.Server.MessageProcessors
{
    internal class BuildTaskCompleteProcessor : MessageProcessorBase<BuildTaskCompleteMessage>
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(BuildTaskCompleteProcessor));


        public override async Task<IResponseMessage> Process(BuildTaskCompleteMessage requestMessage)
        {
            if (!PhotonServer.Instance.TaskRunners.TryGet(requestMessage.TaskId, out var taskRunner)) {
                Log.Warn($"Unable to map response, task '{requestMessage.TaskId}' not found!");
                return null;
            }

            var result = new TaskResult {
                // TODO: This mapping is terrible!
                Successful = requestMessage.Successful,
                Cancelled = false,
                Message = requestMessage.Exception,
            };


            taskRunner.Complete(result);

            return null;
        }
    }
}
