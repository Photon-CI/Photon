using Photon.Agent.Internal;
using Photon.Communication;
using Photon.Communication.Messages;
using Photon.Library.TcpMessages;
using System;
using System.Threading.Tasks;

namespace Photon.Agent.MessageHandlers
{
    public class TaskRunProcessor : MessageProcessorBase<TaskRunRequest>
    {
        public override async Task<IResponseMessage> Process(TaskRunRequest requestMessage)
        {
            if (!PhotonAgent.Instance.Sessions.TryGetSession(requestMessage.AgentSessionId, out var session))
                throw new ApplicationException($"Session '{requestMessage.AgentSessionId}' not found!");

            return new TaskRunResponse {
                Result = await session.RunTaskAsync(requestMessage.TaskName, requestMessage.TaskSessionId),
            };
        }
    }
}
