using Photon.Agent.Internal;
using Photon.Communication;
using Photon.Framework.Messages;
using System;
using System.Threading.Tasks;

namespace Photon.Agent.MessageHandlers
{
    public class BuildTaskStatusProcessor : IProcessMessage<BuildTaskStatusRequest>
    {
        public async Task<IResponseMessage> Process(BuildTaskStatusRequest requestMessage)
        {
            if (!PhotonAgent.Instance.Sessions.TryGetSession(requestMessage.SessionId, out var session))
                return new BuildTaskStartResponse {
                    Successful = false,
                    Exception = $"Session '{requestMessage.SessionId}' not found!",
                };

            var response = new BuildTaskStatusResponse();

            try {
                var task = session.GetTask(requestMessage.TaskId);

                if (task.Output.Length > requestMessage.OutputStart) {
                    response.OutputPosition = task.Output.Length;
                    response.OutputText = task.Output.ToString().Substring(requestMessage.OutputStart);
                }

                response.Complete = task.Complete;
                response.Successful = true;
            }
            catch (Exception error) {
                response.Exception = error.ToString();
            }

            return response;
        }
    }
}
