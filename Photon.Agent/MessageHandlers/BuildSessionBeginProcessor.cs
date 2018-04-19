using Photon.Agent.Internal;
using Photon.Agent.Internal.Session;
using Photon.Communication;
using Photon.Communication.Messages;
using Photon.Library.TcpMessages;
using System.Threading.Tasks;

namespace Photon.Agent.MessageHandlers
{
    public class BuildSessionBeginProcessor : MessageProcessorBase<BuildSessionBeginRequest>
    {
        public override async Task<IResponseMessage> Process(BuildSessionBeginRequest requestMessage)
        {
            var session = new AgentBuildSession(Transceiver, requestMessage.ServerSessionId, requestMessage.SessionClientId) {
                Project = requestMessage.Project,
                AssemblyFilename = requestMessage.AssemblyFile,
                PreBuild = requestMessage.PreBuild,
                GitRefspec = requestMessage.GitRefspec,
                BuildNumber = requestMessage.BuildNumber,
                ServerVariables = requestMessage.Variables,
            };

            PhotonAgent.Instance.Sessions.BeginSession(session);

            try {
                await session.InitializeAsync();

                return new BuildSessionBeginResponse {
                    AgentSessionId = session.SessionId,
                };
            }
            catch {
                await PhotonAgent.Instance.Sessions.ReleaseSessionAsync(session.SessionId);
                throw;
            }
        }
    }
}
