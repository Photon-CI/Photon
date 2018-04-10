using Photon.Agent.Internal;
using Photon.Agent.Internal.Session;
using Photon.Communication;
using System;
using System.Threading.Tasks;
using Photon.Communication.Messages;
using Photon.Library.TcpMessages;

namespace Photon.Agent.MessageHandlers
{
    public class BuildSessionBeginProcessor : MessageProcessorBase<BuildSessionBeginRequest>
    {
        public override async Task<IResponseMessage> Process(BuildSessionBeginRequest requestMessage)
        {
            var response = new BuildSessionBeginResponse();

            try {
                var session = new AgentBuildSession(Transceiver, requestMessage.ServerSessionId) {
                    Project = requestMessage.Project,
                    AssemblyFilename = requestMessage.AssemblyFile,
                    GitRefspec = requestMessage.GitRefspec,
                    TaskName = requestMessage.TaskName,
                    BuildNumber = requestMessage.BuildNumber,
                };

                PhotonAgent.Instance.Sessions.BeginSession(session);

                await session.InitializeAsync();

                response.SessionId = session.SessionId;
                response.Successful = true;
            }
            catch (Exception error) {
                response.Exception = error.Message;
            }

            return response;
        }
    }
}
