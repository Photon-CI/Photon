using Photon.Communication;
using Photon.Communication.Messages;
using Photon.Framework.Server;
using Photon.Framework.TcpMessages;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Photon.Framework.MessageProcessors
{
    internal class BuildTaskOutputProcessor : MessageProcessorBase<BuildTaskOutputMessage>
    {
        public override async Task<IResponseMessage> Process(BuildTaskOutputMessage requestMessage)
        {
            if (!(Transceiver.Context is IServerContext context)) throw new Exception("Context is undefined!");

            var agentSession = context.GetAgentSession(requestMessage.AgentSessionId);

            if (agentSession == null) throw new Exception($"Agent-Session '{requestMessage.AgentSessionId}' not found!");

            if (!agentSession.Tasks.TryGet(requestMessage.TaskId, out var task))
                throw new Exception($"Task '{requestMessage.TaskId}' not found!");

            task.AppendOutput(requestMessage.Text);
            return null;
        }
    }
}
