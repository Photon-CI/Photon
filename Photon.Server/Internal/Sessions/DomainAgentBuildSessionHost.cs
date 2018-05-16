using Photon.Framework.Server;
using Photon.Library.TcpMessages;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Server.Internal.Sessions
{
    internal class DomainAgentBuildSessionHost : DomainAgentSessionHostBase
    {
        private readonly ServerBuildSession session;


        public DomainAgentBuildSessionHost(ServerBuildSession session, ServerAgent agent, CancellationToken token) : base(session, agent, token)
        {
            this.session = session;
        }

        protected override async Task OnBeginSession(CancellationToken token)
        {
            var message = new BuildSessionBeginRequest {
                ServerSessionId = session.SessionId,
                SessionClientId = SessionClientId,
                Project = session.Project,
                Agent = Agent,
                AssemblyFile = session.AssemblyFilename,
                PreBuild = session.PreBuild,
                GitRefspec = session.GitRefspec,
                BuildNumber = session.BuildNumber,
                Variables = session.Variables,
                Commit = session.Commit,
            };

            var response = await MessageClient.Send(message)
                .GetResponseAsync<BuildSessionBeginResponse>(token);

            AgentSessionId = response.AgentSessionId;
        }

        protected override async Task OnReleaseSessionAsync(CancellationToken token)
        {
            var message = new BuildSessionReleaseRequest {
                AgentSessionId = AgentSessionId,
            };

            await MessageClient.Send(message)
                .GetResponseAsync(token);
        }

        protected override void OnSessionOutput(string text)
        {
            session.Output.AppendRaw(text);
        }
    }
}
