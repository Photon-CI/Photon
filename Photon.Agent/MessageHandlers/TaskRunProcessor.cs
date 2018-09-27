using log4net;
using Photon.Agent.Internal;
using Photon.Agent.Internal.Session;
using Photon.Communication;
using Photon.Communication.Messages;
using Photon.Library.TcpMessages;
using System;
using System.Threading.Tasks;

namespace Photon.Agent.MessageHandlers
{
    public class TaskRunProcessor : MessageProcessorBase<TaskRunRequest>
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(TaskRunProcessor));


        public override async Task<IResponseMessage> Process(TaskRunRequest requestMessage)
        {
            var context = PhotonAgent.Instance.Context;

            if (!context.Sessions.TryGet(requestMessage.AgentSessionId, out var session))
                throw new ApplicationException($"Session '{requestMessage.AgentSessionId}' not found!");

            if (!(session is AgentDeploySession deploySession))
                throw new ApplicationException("Execution of individual tasks is only available on deployment sessions!");

            try {
                await deploySession.RunTaskAsync(requestMessage.TaskName, requestMessage.TaskSessionId);

                return new TaskRunResponse();
            }
            catch (Exception error) {
                Log.Error($"Failed to run task '{requestMessage.TaskName}'!", error);
                throw new ApplicationException($"Failed to run task '{requestMessage.TaskName}'!", error);
            }
        }
    }
}
