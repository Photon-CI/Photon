using Photon.Agent.Internal;
using Photon.Communication;
using Photon.Communication.Messages;
using Photon.Library.TcpMessages.Packages;
using System;
using System.Threading.Tasks;

namespace Photon.Agent.MessageHandlers.Packages
{
    internal class ProjectPackagePushProcessor : MessageProcessorBase<WorkerProjectPackagePushRequest>
    {
        public override async Task<IResponseMessage> Process(WorkerProjectPackagePushRequest request)
        {
            var context = PhotonAgent.Instance.Context;

            if (!context.Sessions.TryGet(request.AgentSessionId, out var session))
                throw new ApplicationException($"Session '{request.AgentSessionId}' not found!");

            var agentRequest = new AgentProjectPackagePushRequest {
                Filename = request.Filename,
            };

            await session.ServerTransceiver.Send(agentRequest)
                .GetResponseAsync(session.Token);

            return new ResponseMessageBase();
        }
    }
}
