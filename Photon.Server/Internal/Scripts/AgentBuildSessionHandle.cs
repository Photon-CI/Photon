using Photon.Communication;
using Photon.Framework;
using Photon.Framework.Scripts;
using Photon.Library.Messages;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Photon.Server.Internal.Scripts
{
    public class AgentBuildSessionHandle : IAgentSessionHandle, IDisposable
    {
        private readonly ServerAgentDefinition definition;
        private readonly MessageProcessor messageProcessor;
        private readonly MessageClient messageClient;

        private string sessionId;


        public AgentBuildSessionHandle(ServerAgentDefinition agentDefinition)
        {
            this.definition = agentDefinition;

            messageProcessor = new MessageProcessor();
            messageProcessor.Scan(Assembly.GetExecutingAssembly());

            messageClient = new MessageClient(messageProcessor);
        }

        public void Dispose()
        {
            messageClient?.Dispose();
        }

        public async Task BeginAsync()
        {
            messageProcessor.Start();

            await messageClient.ConnectAsync(definition.TcpHost, definition.TcpPort);

            var message = new BuildSessionBeginRequest();

            var handle = messageClient.Send(message);
            var response = await handle.GetResponseAsync<BuildSessionBeginResponse>();

            sessionId = response?.SessionId;
            if (string.IsNullOrEmpty(sessionId))
                throw new ApplicationException("Failed to begin agent session!");
        }

        public async Task ReleaseAsync()
        {
            // TODO: Locking on sessionId and isActive

            var message = new BuildSessionReleaseRequest {
                SessionId = sessionId,
            };

            var handle = messageClient.Send(message);
            var response = await handle.GetResponseAsync<BuildSessionReleaseResponse>();

            if (!(response?.Successful ?? false))
                throw new ApplicationException("Failed to release agent session!");

            await messageClient.DisconnectAsync();

            await messageProcessor.StopAsync();
        }

        public Task RunTaskAsync(string taskName)
        {
            // TODO: this

            throw new NotImplementedException();
        }
    }
}
