using Photon.Agent.Internal;
using Photon.Agent.Internal.Session;
using Photon.Communication;
using Photon.Framework.Messages;
using System.Threading.Tasks;

namespace Photon.Agent.MessageHandlers
{
    public class BuildSessionBeginProcessor : MessageProcessorBase<BuildSessionBeginRequest>
    {
        public override async Task<IResponseMessage> Process(BuildSessionBeginRequest requestMessage)
        {
            var session = new AgentBuildSession(Transceiver, requestMessage.ServerSessionId) {
                Project = requestMessage.Project,
                AssemblyFile = requestMessage.AssemblyFile,
                GitRefspec = requestMessage.GitRefspec,
                TaskName = requestMessage.TaskName,
            };

            PhotonAgent.Instance.Sessions.BeginSession(session);

            await session.InitializeAsync();

            return new BuildSessionBeginResponse {
                SessionId = session.SessionId,
            };
        }
    }
}
