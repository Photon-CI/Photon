using Photon.Framework;
using Photon.Library.TcpMessages;
using System.Threading.Tasks;

namespace Photon.Server.Internal.Sessions
{
    internal class DomainAgentDeploySessionHost : DomainAgentSessionHostBase
    {
        private readonly ServerDeploySession session;


        public DomainAgentDeploySessionHost(ServerDeploySession session, ServerAgentDefinition agent) : base(session, agent)
        {
            this.session = session;
        }

        protected override async Task OnBeginSession()
        {
            var message = new DeploySessionBeginRequest {
                ServerSessionId = session.SessionId,
                SessionClientId = SessionClientId,
                ProjectPackageId = session.ProjectPackageId,
                ProjectPackageVersion = session.ProjectPackageVersion,
            };

            var response = await MessageClient.Send(message)
                .GetResponseAsync<DeploySessionBeginResponse>();

            AgentSessionId = response.AgentSessionId;
        }

        protected override async Task OnReleaseSessionAsync()
        {
            var message = new BuildSessionReleaseRequest {
                AgentSessionId = AgentSessionId,
            };

            await MessageClient.Send(message)
                .GetResponseAsync();
        }

        protected override void OnSessionOutput(string text)
        {
            session.Output.AppendRaw(text);
        }
    }
}
