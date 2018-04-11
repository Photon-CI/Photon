using Photon.Agent.Internal;
using Photon.Communication;
using Photon.Communication.Messages;
using Photon.Framework.TcpMessages;
using System;
using System.Threading.Tasks;

namespace Photon.Agent.MessageHandlers
{
    public class BuildTaskRunProcessor : MessageProcessorBase<BuildTaskRunRequest>
    {
        public override async Task<IResponseMessage> Process(BuildTaskRunRequest requestMessage)
        {
            if (!PhotonAgent.Instance.Sessions.TryGetSession(requestMessage.AgentSessionId, out var session))
                throw new ApplicationException($"Session '{requestMessage.AgentSessionId}' not found!");

            return new BuildTaskRunResponse {
                Result = await session.RunTaskAsync(requestMessage.TaskName, requestMessage.TaskSessionId),
            };
        }
    }
}
