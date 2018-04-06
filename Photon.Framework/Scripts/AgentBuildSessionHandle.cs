using Photon.Communication;
using Photon.Framework.Messages;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Photon.Framework.Scripts
{
    public class AgentBuildSessionHandle : IAgentSessionHandle, IDisposable
    {
        private readonly IServerBuildContext scriptContext;
        private readonly ServerAgentDefinition definition;
        private readonly MessageProcessor messageProcessor;
        private readonly MessageClient messageClient;

        private string sessionId;


        public AgentBuildSessionHandle(IServerBuildContext scriptContext, ServerAgentDefinition agentDefinition)
        {
            this.scriptContext = scriptContext;
            this.definition = agentDefinition;

            messageProcessor = new MessageProcessor();
            messageProcessor.Scan(Assembly.GetExecutingAssembly());

            messageClient = new MessageClient(messageProcessor);
        }

        public void Dispose()
        {
            messageClient?.Dispose();
        }

        public async Task BeginAsync(string packageId, string packageVersion)
        {
            messageProcessor.Start();

            await messageClient.ConnectAsync(definition.TcpHost, definition.TcpPort);

            var message = new BuildSessionBeginRequest {
                Project = scriptContext.Project,
                AssemblyFile = scriptContext.AssemblyFile,
                PackageId = packageId,
                PackageVersion = packageVersion,
            };

            var handle = messageClient.Send(message);
            var response = await handle.GetResponseAsync<BuildSessionBeginResponse>();

            sessionId = response?.SessionId;
            if (string.IsNullOrEmpty(sessionId))
                throw new ApplicationException("Failed to begin agent session!");
        }

        public async Task ReleaseAsync()
        {
            // TODO: Locking on sessionId and isActive

            if (messageClient.IsConnected) {
                var message = new BuildSessionReleaseRequest {
                    SessionId = sessionId,
                };

                var handle = messageClient.Send(message);
                var response = await handle.GetResponseAsync<BuildSessionReleaseResponse>();

                if (!(response?.Successful ?? false))
                    throw new ApplicationException("Failed to release agent session!");

                await messageClient.DisconnectAsync();
            }

            await messageProcessor.StopAsync();
        }

        public async Task RunTaskAsync(string taskName)
        {
            var message = new BuildTaskRunRequest {
                SessionId = sessionId,
                TaskName = taskName,
            };

            var handle = messageClient.Send(message);
            var response = await handle.GetResponseAsync<BuildTaskRunResponse>();

            scriptContext.Output
                .Append("Running Task ", ConsoleColor.DarkCyan)
                .Append(taskName, ConsoleColor.Cyan)
                .Append(" on Agent ", ConsoleColor.DarkCyan)
                .Append(definition.Name, ConsoleColor.Cyan)
                .AppendLine("...", ConsoleColor.DarkCyan);

            if (!response.Successful) {
                scriptContext.Output
                    .AppendLine($"Task '{taskName}' Failed!", ConsoleColor.Red)
                    .AppendLine(response.Exception, ConsoleColor.DarkYellow);
            }

            scriptContext.Output
                .Append("Task ", ConsoleColor.DarkGreen)
                .Append(taskName, ConsoleColor.Green)
                .Append(" completed successfully.", ConsoleColor.DarkGreen);
        }
    }
}
