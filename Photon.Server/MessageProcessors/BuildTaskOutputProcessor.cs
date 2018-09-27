using Photon.Communication;
using Photon.Communication.Messages;
using Photon.Library.TcpMessages;
using Photon.Server.Internal;
using System;
using System.Threading.Tasks;

namespace Photon.Server.MessageProcessors
{
    internal class BuildTaskOutputProcessor : MessageProcessorBase<TaskOutputMessage>
    {
        public override async Task<IResponseMessage> Process(TaskOutputMessage requestMessage)
        {
            var serverContext = PhotonServer.Instance.Context;

            //if (!(Transceiver.Context is ServerSessionBase session)) throw new Exception("Session is undefined!");

            if (!serverContext.Sessions.TryGet(requestMessage.ServerSessionId, out var session))
                throw new Exception($"Server session '{requestMessage.ServerSessionId}' not found!");

            //if (!session.GetAgentSession(requestMessage.SessionClientId, out var sessionHost))
            //    throw new Exception($"Agent Session Client ID '{requestMessage.SessionClientId}' not found!");

            //if (!sessionHost.Tasks.TryGet(requestMessage.TaskId, out var task))
            //    throw new Exception($"Task '{requestMessage.TaskId}' not found!");

            //await Task.Run(() => task.AppendOutput(requestMessage.Text));
            return null;
        }
    }
}
