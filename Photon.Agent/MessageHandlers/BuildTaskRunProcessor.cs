using Photon.Agent.Internal;
using Photon.Communication;
using System;
using System.Threading.Tasks;
using Photon.Communication.Messages;
using Photon.Library.TcpMessages;

namespace Photon.Agent.MessageHandlers
{
    public class BuildTaskRunProcessor : MessageProcessorBase<BuildTaskRunRequest>
    {
        public override async Task<IResponseMessage> Process(BuildTaskRunRequest requestMessage)
        {
            if (!PhotonAgent.Instance.Sessions.TryGetSession(requestMessage.AgentSessionId, out var session))
                return new BuildTaskRunResponse {
                    Successful = false,
                    Exception = $"Session '{requestMessage.AgentSessionId}' not found!",
                };

            var response = new BuildTaskRunResponse();

            try {
                response.Result = await session.RunTaskAsync(requestMessage.TaskName, requestMessage.TaskSessionId);
                response.Successful = true;
            }
            catch (Exception error) {
                response.Exception = error.ToString();
            }

            return response;
        }
    }
}
