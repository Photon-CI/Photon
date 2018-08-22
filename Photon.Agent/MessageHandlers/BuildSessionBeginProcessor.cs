using log4net;
using Photon.Agent.Internal;
using Photon.Agent.Internal.Session;
using Photon.Communication;
using Photon.Communication.Messages;
using Photon.Library.TcpMessages;
using System;
using System.Threading.Tasks;

namespace Photon.Agent.MessageHandlers
{
    public class BuildSessionBeginProcessor : MessageProcessorBase<BuildSessionBeginRequest>
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(BuildSessionBeginProcessor));


        public override async Task<IResponseMessage> Process(BuildSessionBeginRequest requestMessage)
        {
            var session = new AgentBuildSession(Transceiver, requestMessage.ServerSessionId, requestMessage.SessionClientId) {
                Project = requestMessage.Project,
                AssemblyFilename = requestMessage.AssemblyFile,
                PreBuild = requestMessage.PreBuild,
                TaskName = requestMessage.TaskName,
                GitRefspec = requestMessage.GitRefspec,
                BuildNumber = requestMessage.BuildNumber,
                ServerVariables = requestMessage.Variables,
                SourceCommit = requestMessage.Commit,
                Agent = requestMessage.Agent,
            };

            PhotonAgent.Instance.Sessions.BeginSession(session);

            try {
                await session.InitializeAsync();

                return new BuildSessionBeginResponse {
                    AgentSessionId = session.SessionId,
                };
            }
            catch (Exception error) {
                Log.Error("Failed to initialize Build Session!", error);

                await session.CompleteAsync();
                await PhotonAgent.Instance.Sessions.ReleaseSessionAsync(session.SessionId);

                throw new ApplicationException("Failed to initialize Build Session!", error);
            }
        }
    }
}
