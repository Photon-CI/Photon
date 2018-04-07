using Photon.Agent.Internal;
using Photon.Communication;
using Photon.Framework.Messages;
using System;
using System.Threading.Tasks;
using Photon.Library.Messages;

namespace Photon.Agent.MessageHandlers
{
    public class BuildTaskStartProcessor : MessageProcessorBase<BuildTaskStartRequest>
    {
        public override async Task<IResponseMessage> Process(BuildTaskStartRequest requestMessage)
        {
            if (!PhotonAgent.Instance.Sessions.TryGetSession(requestMessage.SessionId, out var session))
                return new BuildTaskStartResponse {
                    Successful = false,
                    Exception = $"Session '{requestMessage.SessionId}' not found!",
                };

            var response = new BuildTaskStartResponse();

            try {
                var taskHandle = session.BeginTask(requestMessage.TaskName);
                response.TaskId = taskHandle.SessionId;
                response.Successful = true;
            }
            catch (Exception error) {
                response.Exception = error.ToString();
            }

            return response;
        }
    }
}
