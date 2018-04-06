using Photon.Agent.Internal;
using Photon.Communication;
using Photon.Framework.Messages;
using System;
using System.Threading.Tasks;

namespace Photon.Agent.MessageHandlers
{
    public class BuildTaskRunProcessor : IProcessMessage<BuildTaskRunRequest>
    {
        public async Task<IResponseMessage> Process(BuildTaskRunRequest requestMessage)
        {
            if (!PhotonAgent.Instance.Sessions.TryGetSession(requestMessage.SessionId, out var session))
                return new BuildTaskRunResponse {
                    Successful = false,
                    Output = $"Session '{requestMessage.SessionId}' not found!",
                };

            var response = new BuildTaskRunResponse();

            try {
                await session.RunTaskAsync(requestMessage.TaskName);
                response.Successful = true;
            }
            catch (Exception error) {
                response.Exception = error.ToString();
            }

            return response;
        }
    }
}
