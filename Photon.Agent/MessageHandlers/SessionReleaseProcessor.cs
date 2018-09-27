using Photon.Agent.Internal;
using Photon.Communication;
using Photon.Communication.Messages;
using Photon.Library.TcpMessages;
using System;
using System.Threading.Tasks;

namespace Photon.Agent.MessageHandlers
{
    public class SessionReleaseProcessor : MessageProcessorBase<SessionReleaseRequest>
    {
        public override async Task<IResponseMessage> Process(SessionReleaseRequest requestMessage)
        {
            var context = PhotonAgent.Instance.Context;

            if (!context.Sessions.TryGet(requestMessage.AgentSessionId, out var session))
                throw new ApplicationException("");

            await session.ReleaseAsync();

            var _ = Task.Delay(800).ContinueWith(async t => {
                await context.Sessions.ReleaseSessionAsync(requestMessage.AgentSessionId);
            });

            return null;
        }
    }
}
