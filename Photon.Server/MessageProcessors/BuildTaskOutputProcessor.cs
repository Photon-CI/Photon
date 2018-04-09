using log4net;
using Photon.Communication;
using Photon.Communication.Messages;
using Photon.Server.Internal;
using System.Threading.Tasks;
using Photon.Framework.TcpMessages;

namespace Photon.Server.MessageProcessors
{
    internal class BuildTaskOutputProcessor : MessageProcessorBase<BuildTaskOutputMessage>
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(BuildTaskOutputProcessor));


        public override async Task<IResponseMessage> Process(BuildTaskOutputMessage requestMessage)
        {
            if (!PhotonServer.Instance.TaskRunners.TryGet(requestMessage.TaskSessionId, out var taskRunner)) {
                Log.Warn($"Unable to map response, TaskRunner Session '{requestMessage.TaskSessionId}' not found!");
                return null;
            }

            taskRunner.AppendOutput(requestMessage.Text);

            return null;
        }
    }
}
