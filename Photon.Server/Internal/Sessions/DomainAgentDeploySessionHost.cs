using System;
using Photon.Framework.Server;
using Photon.Library.TcpMessages;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Server.Internal.Sessions
{
    internal class DomainAgentDeploySessionHost : DomainAgentSessionHostBase
    {
        private readonly ServerDeploySession session;


        public DomainAgentDeploySessionHost(ServerDeploySession session, ServerAgent agent, CancellationToken token) : base(session, agent, token)
        {
            this.session = session;
        }

        protected override async Task OnBeginSession(CancellationToken token)
        {
            var message = new DeploySessionBeginRequest {
                DeploymentNumber = session.Deployment.Number,
                Project = session.Project,
                Agent = Agent,
                ServerSessionId = session.SessionId,
                SessionClientId = SessionClientId,
                ProjectPackageId = session.ProjectPackageId,
                ProjectPackageVersion = session.ProjectPackageVersion,
                Variables = session.Variables,
                EnvironmentName = session.EnvironmentName,
            };

            var response = await MessageClient.Send(message)
                .GetResponseAsync<DeploySessionBeginResponse>(token);

            AgentSessionId = response.AgentSessionId;
        }

        protected override async Task OnReleaseSessionAsync(CancellationToken token)
        {
            var message = new SessionReleaseRequest {
                AgentSessionId = AgentSessionId,
            };

            MessageClient.SendOneWay(message);
                //.GetResponseAsync(token);

            MessageClient.Disconnect(TimeSpan.FromSeconds(30));

            // TODO:
            // 1) sender sends a release message
            // 2) receiver sends released message
            // 3) flush receiver
            // 4) sender waits up to N seconds for response
        }

        protected override void OnSessionOutput(string text)
        {
            session.Output.WriteRaw(text);
        }
    }
}
