using Photon.Agent.Internal;
using Photon.Communication;
using Photon.Library.Messages;
using System.Threading.Tasks;
using Photon.Agent.Internal.Tasks;

namespace Photon.Agent.MessageHandlers
{
    public class BuildSessionBeginProcessor : IProcessMessage<BuildSessionBeginRequest>
    {
        public async Task<IResponseMessage> Process(BuildSessionBeginRequest requestMessage)
        {
            var agent = PhotonAgent.Instance.Definition;

            var context = new AgentBuildContext(agent) {
                Project = requestMessage.Project,
                PackageId = requestMessage.PackageId,
                PackageVersion = requestMessage.PackageVersion,
            };

            var session = new AgentBuildSession(context);

            PhotonAgent.Instance.Sessions.BeginSession(session);

            return new BuildSessionBeginResponse {
                SessionId = session.SessionId,
            };
        }
    }
}
