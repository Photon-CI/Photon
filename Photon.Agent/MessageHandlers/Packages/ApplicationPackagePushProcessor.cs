using Photon.Agent.Internal;
using Photon.Communication;
using Photon.Communication.Messages;
using Photon.Library.TcpMessages.Packages;
using System;
using System.Threading.Tasks;

namespace Photon.Agent.MessageHandlers.Packages
{
    internal class ApplicationPackagePushProcessor : MessageProcessorBase<WorkerApplicationPackagePushRequest>
    {
        public override async Task<IResponseMessage> Process(WorkerApplicationPackagePushRequest request)
        {
            if (!PhotonAgent.Instance.Context.Sessions.TryGet(request.AgentSessionId, out var session))
                throw new ApplicationException($"Session '{request.AgentSessionId}' not found!");

            var agentRequest = new AgentApplicationPackagePushRequest {
                Filename = request.Filename,
            };

            await session.ServerTransceiver.Send(agentRequest)
                .GetResponseAsync(session.Token);

            return new ResponseMessageBase();
        }
    }
}
