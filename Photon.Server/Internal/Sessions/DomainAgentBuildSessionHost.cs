using Photon.Framework;
using Photon.Library.TcpMessages;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Server.Internal.Sessions
{
    internal class DomainAgentBuildSessionHost : DomainAgentSessionHostBase
    {
        private readonly ServerBuildSession session;


        public DomainAgentBuildSessionHost(ServerBuildSession session, ServerAgentDefinition agent, CancellationToken token) : base(session, agent, token)
        {
            this.session = session;
        }

        protected override async Task OnBeginSession()
        {
            var message = new BuildSessionBeginRequest {
                ServerSessionId = session.SessionId,
                SessionClientId = SessionClientId,
                Project = session.Project,
                AssemblyFile = session.AssemblyFilename,
                PreBuild = session.PreBuild,
                GitRefspec = session.GitRefspec,
                BuildNumber = session.BuildNumber,
                Variables = session.Variables,
                Commit = session.Commit,
            };

            var response = await MessageClient.Send(message)
                .GetResponseAsync<BuildSessionBeginResponse>();

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
