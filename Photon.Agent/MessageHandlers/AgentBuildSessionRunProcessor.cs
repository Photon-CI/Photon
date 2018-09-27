using Photon.Agent.Internal;
using Photon.Agent.Internal.Session;
using Photon.Communication;
using Photon.Communication.Messages;
using Photon.Library.Communication.Messages.Session;
using System.Threading.Tasks;

namespace Photon.Agent.MessageHandlers
{
    internal class AgentBuildSessionRunProcessor : IProcessMessage<AgentBuildSessionRunRequest>
    {
        public MessageTransceiver Transceiver {get; set;}


        public async Task<IResponseMessage> Process(AgentBuildSessionRunRequest requestMessage)
        {
            var context = PhotonAgent.Instance.Context;

            var buildSession = new AgentBuildSession(Transceiver) {
                ServerSessionId = requestMessage.ServerSessionId,
                Project = requestMessage.Project,
                AssemblyFilename = requestMessage.AssemblyFilename,
                ServerVariables = requestMessage.ServerVariables,
                TaskName = requestMessage.TaskName,
                GitRefspec = requestMessage.GitRefspec,
                BuildNumber = requestMessage.BuildNumber,
                PreBuild = requestMessage.PreBuildCommand,
                //CommitHash = requestMessage.CommitHash,
            };

            await buildSession.InitializeAsync();

            context.Sessions.BeginSession(buildSession);

            try {
                await buildSession.RunTaskAsync();
            }
            finally {
                await buildSession.ReleaseAsync();
            }

            return new AgentBuildSessionRunResponse {
                //...
            };
        }
    }
}
