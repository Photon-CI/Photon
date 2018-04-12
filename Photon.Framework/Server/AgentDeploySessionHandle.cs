using Photon.Communication;
using Photon.Framework.Tasks;
using Photon.Framework.TcpMessages;
using System;
using System.Threading.Tasks;

namespace Photon.Framework.Server
{
    public class AgentDeploySessionHandle : IAgentSessionHandle
    {
        private readonly IServerDeployContext context;
        private readonly ServerAgentDefinition definition;
        private readonly MessageClient messageClient;

        public TaskRunnerManager Tasks {get;}
        public string AgentSessionId {get; private set;}


        public AgentDeploySessionHandle(IServerDeployContext context, ServerAgentDefinition agentDefinition, MessageProcessorRegistry registry)
        {
            this.context = context;
            this.definition = agentDefinition;

            Tasks = new TaskRunnerManager();

            messageClient = new MessageClient(registry) {
                Context = context,
            };

            messageClient.ThreadException += MessageClient_OnThreadException;
        }

        public void Dispose()
        {
            Tasks?.Dispose();
            messageClient?.Dispose();
        }

        public async Task BeginAsync()
        {
            try {
                await messageClient.ConnectAsync(definition.TcpHost, definition.TcpPort);

                context.Output
                    .Append("Connected to Agent ", ConsoleColor.DarkGreen)
                    .AppendLine(definition.Name, ConsoleColor.Green);
            }
            catch (Exception error) {
                context.Output
                    .Append("Failed to connect to Agent ", ConsoleColor.DarkYellow)
                    .Append(definition.Name, ConsoleColor.Yellow)
                    .AppendLine($"! {error.Message}", ConsoleColor.DarkYellow);

                throw new ApplicationException($"Failed to connect to Agent '{definition.Name}'! {error.Message}");
            }

            var message = new DeploySessionBeginRequest {
                ServerSessionId = context.ServerSessionId,
                ProjectPackageId = context.ProjectPackageId,
                ProjectPackageVersion = context.ProjectPackageVersion,
            };

            try {
                var response = await messageClient.Send(message)
                    .GetResponseAsync<DeploySessionBeginResponse>();

                AgentSessionId = response.AgentSessionId;
            }
            catch (Exception error) {
                throw new ApplicationException($"Failed to start Agent Session! {error.Message}");
            }

            //if (string.IsNullOrEmpty(AgentSessionId))
            //    throw new ApplicationException("Failed to begin agent session!");
        }

        public async Task ReleaseAsync()
        {
            // TODO: Locking on sessionId and isActive

            if (messageClient.IsConnected) {
                var message = new DeploySessionReleaseRequest {
                    SessionId = AgentSessionId,
                };

                try {
                    await messageClient.Send(message)
                        .GetResponseAsync();
                }
                catch (Exception error) {
                    throw new ApplicationException($"Failed to release agent session! {error.Message}");
                }

                await messageClient.DisconnectAsync();
            }
        }

        public async Task RunTaskAsync(string taskName)
        {
            context.Output
                .Append("Running Deploy-Task ", ConsoleColor.DarkCyan)
                .Append(taskName, ConsoleColor.Cyan)
                .Append(" on Agent ", ConsoleColor.DarkCyan)
                .Append(definition.Name, ConsoleColor.Cyan)
                .AppendLine("...", ConsoleColor.DarkCyan);

            var runner = new TaskRunner(messageClient, AgentSessionId);
            runner.OutputEvent += (o, e) => {
                context.Output.AppendRaw(e.Text);
            };

            Tasks.Add(runner);

            var result = await runner.Run(taskName);

            if (!result.Successful) {
                context.Output
                    .AppendLine($"Deploy-Task '{taskName}' Failed!", ConsoleColor.Red)
                    .AppendLine(result.Message, ConsoleColor.DarkYellow);
            }

            context.Output
                .Append("Deploy-Task ", ConsoleColor.DarkGreen)
                .Append(taskName, ConsoleColor.Green)
                .AppendLine(" completed successfully.", ConsoleColor.DarkGreen);
        }

        public TaskRunner GetTaskRunner(string agentTaskId)
        {
            return Tasks.TryGet(agentTaskId, out var taskRunner) ? taskRunner
                : throw new ApplicationException($"Task '{agentTaskId}' not found!");
        }

        private void MessageClient_OnThreadException(object sender, UnhandledExceptionEventArgs e)
        {
            var x = (Exception)e.ExceptionObject;

            context.Output
                .Append("TCP Error!", ConsoleColor.Red)
                .AppendLine(x.Message, ConsoleColor.DarkRed);
        }
    }
}
