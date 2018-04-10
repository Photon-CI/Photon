using Photon.Communication;
using Photon.Framework.TcpMessages;
using System;
using System.Threading.Tasks;

namespace Photon.Framework.Scripts
{
    public class AgentDeploySessionHandle : IDisposable, IAgentSessionHandle
    {
        private readonly ServerAgentDefinition definition;
        private readonly MessageClient messageClient;

        private string agentSessionId;

        //public IServerDeployContext Context {get;}
        public string ServerSessionId {get; set;}
        public string ProjectPackageId {get; set;}
        public string ProjectPackageVersion {get; set;}
        public ScriptOutput Output {get; set;}


        public AgentDeploySessionHandle(IServerDeployContext context, ServerAgentDefinition agentDefinition, MessageProcessorRegistry registry)
        {
            this.definition = agentDefinition;

            ProjectPackageId = context.ProjectPackageId;
            ProjectPackageVersion = context.ProjectPackageVersion;
            Output = context.Output;

            messageClient = new MessageClient(registry) {
                Context = context,
            };

            messageClient.ThreadException += MessageClient_OnThreadException;
        }

        private void MessageClient_OnThreadException(object sender, UnhandledExceptionEventArgs e)
        {
            var x = (Exception)e.ExceptionObject;
        }

        public void Dispose()
        {
            messageClient?.Dispose();
        }

        public async Task BeginAsync(string packageId, string packageVersion)
        {
            try {
                await messageClient.ConnectAsync(definition.TcpHost, definition.TcpPort);
                Output.Append("Connected to Agent ", ConsoleColor.DarkGreen)
                    .AppendLine(definition.Name, ConsoleColor.Green);
            }
            catch (Exception error) {
                Output.Append("Failed to connect to Agent ", ConsoleColor.DarkYellow)
                    .Append(definition.Name, ConsoleColor.Yellow)
                    .AppendLine($"! {error.Message}", ConsoleColor.DarkYellow);

                throw new ApplicationException($"Failed to connect to Agent '{definition.Name}'! {error.Message}");
            }

            var message = new DeploySessionBeginRequest {
                ServerSessionId = ServerSessionId,
                ProjectPackageId = ProjectPackageId,
                ProjectPackageVersion = ProjectPackageVersion,
            };

            var handle = messageClient.Send(message);
            var response = await handle.GetResponseAsync<DeploySessionBeginResponse>();

            if (!response.Successful)
                throw new ApplicationException($"Failed to start Agent Session! {response.Exception}");

            agentSessionId = response.SessionId;
            if (string.IsNullOrEmpty(agentSessionId))
                throw new ApplicationException("Failed to begin agent session!");
        }

        public async Task ReleaseAsync()
        {
            // TODO: Locking on sessionId and isActive

            if (messageClient.IsConnected) {
                var message = new DeploySessionReleaseRequest {
                    SessionId = agentSessionId,
                };

                var handle = messageClient.Send(message);
                var response = await handle.GetResponseAsync<DeploySessionReleaseResponse>();

                if (!(response?.Successful ?? false))
                    throw new ApplicationException("Failed to release agent session!");

                await messageClient.DisconnectAsync();
            }
        }

        public async Task RunTaskAsync(string taskName)
        {
            Output
                .Append("Running Task ", ConsoleColor.DarkCyan)
                .Append(taskName, ConsoleColor.Cyan)
                .Append(" on Agent ", ConsoleColor.DarkCyan)
                .Append(definition.Name, ConsoleColor.Cyan)
                .AppendLine("...", ConsoleColor.DarkCyan);

            throw new NotImplementedException();

            //var runner = new TaskRunner(messageClient, agentSessionId);
            //runner.OutputEvent += (o, e) => {
            //    Output.AppendRaw(e.Text);
            //};

            //PhotonServer.Instance.TaskRunners.Add(runner);

            //var result = await runner.Run(taskName);

            //if (!result.Successful) {
            //    Output
            //        .AppendLine($"Task '{taskName}' Failed!", ConsoleColor.Red)
            //        .AppendLine(result.Message, ConsoleColor.DarkYellow);
            //}

            Output
                .Append("Task ", ConsoleColor.DarkGreen)
                .Append(taskName, ConsoleColor.Green)
                .Append(" completed successfully.", ConsoleColor.DarkGreen);
        }
    }
}
