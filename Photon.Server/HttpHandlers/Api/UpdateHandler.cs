using PiServerLite.Http.Handlers;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Photon.Communication;
using Photon.Framework;
using Photon.Library.TcpMessages;
using Photon.Server.Internal;

namespace Photon.Server.HttpHandlers.Api
{
    [HttpHandler("/api/update")]
    internal class UpdateHandler : HttpHandlerAsync
    {
        private string LatestAgentVersion;


        public override async Task<HttpHandlerResult> PostAsync()
        {
            using (var webClient = new WebClient()) {
                var versionUrl = "http://photon.null511.info/api/agent/version";
                LatestAgentVersion = await webClient.DownloadStringTaskAsync(versionUrl);
            }

            var queue = new ActionBlock<ServerAgentDefinition>(AgentAction);

            foreach (var agent in PhotonServer.Instance.Definition.Agents)
                queue.Post(agent);

            queue.Complete();
            await queue.Completion;

            return Ok().SetText("All agents updated successfully.");
        }

        private async Task AgentAction(ServerAgentDefinition agent)
        {
            using (var messageClient = new MessageClient(PhotonServer.Instance.MessageRegistry)) {
                await messageClient.ConnectAsync(agent.TcpHost, agent.TcpPort, CancellationToken.None);

                var agentVersionRequest = new AgentGetVersionRequest();

                var agentVersionResponse = await messageClient.Send(agentVersionRequest)
                    .GetResponseAsync<AgentGetVersionResponse>();

                if (!HasUpdates(agentVersionResponse.Version, LatestAgentVersion)) {
                    // TODO: Print Up-To-Date message
                    return;
                }

                await UpdateAgent(agent, messageClient);
            }
        }

        private async Task UpdateAgent(ServerAgentDefinition agent, MessageClient messageClient)
        {
            var updater = new AgentUpdater();
            //updater.UpdateVersion = ;
            //updater.UpdateMsiFilename = ;
            //updater.Output = Context.

            throw new NotImplementedException();
        }

        private static bool HasUpdates(string versionCurrent, string versionLatest)
        {
            throw new NotImplementedException();
        }
    }
}
