using log4net;
using Photon.Communication;
using Photon.Framework;
using Photon.Framework.Server;
using Photon.Framework.Tasks;
using Photon.Library.TcpMessages;
using System;
using System.Threading.Tasks;

namespace Photon.Server.Internal.Sessions
{
    public class AgentBuildSessionHandle : IAgentSessionHandle
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(AgentBuildSessionHandle));

        private readonly IServerBuildContext context;
        private readonly ServerAgentDefinition definition;
        private readonly MessageClient messageClient;

        public TaskRunnerManager Tasks {get;}
        public string AgentSessionId {get; private set;}


        public AgentBuildSessionHandle(IServerBuildContext context, ServerAgentDefinition agentDefinition, MessageProcessorRegistry registry)
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
                Log.Info($"Connected to Agent '{definition.Name}'.");
            }
            catch (Exception error) {
                Log.Error($"Failed to connect to Agent '{definition.Name}'!", error);
                throw new ApplicationException($"Failed to connect to Agent '{definition.Name}'! {error.Message}");
            }

            var message = new BuildSessionBeginRequest {
                ServerSessionId = context.ServerSessionId,
                Project = context.Project,
                AssemblyFile = context.AssemblyFilename,
                GitRefspec = context.GitRefspec,
                BuildNumber = context.BuildNumber,
            };


            try {
                var response = await messageClient.Send(message)
                    .GetResponseAsync<BuildSessionBeginResponse>();

                AgentSessionId = response.SessionId;
            }
            catch (Exception error) {
                throw new ApplicationException($"Failed to start Agent Session! {error.Message}");
            }                

            //if (string.IsNullOrEmpty(AgentSessionId))
            //    throw new ApplicationException("Failed to begin agent session!");

            Tasks.Start();
        }

        public async Task ReleaseAsync()
        {
            // TODO: Locking on sessionId and isActive

            Tasks.Stop();

            if (messageClient.IsConnected) {
                if (!string.IsNullOrEmpty(AgentSessionId)) {
                    var message = new BuildSessionReleaseRequest {
                        SessionId = AgentSessionId,
                    };

                    try {
                        await messageClient.Send(message)
                            .GetResponseAsync();
                    }
                    catch (Exception error) {
                        Log.Error($"Failed to release Agent Session '{AgentSessionId}'! {error.Message}");
                    }
                }

                await messageClient.DisconnectAsync();
            }
        }

        public async Task RunTaskAsync(string taskName)
        {
            context.Output
                .Append("Running Build-Task ", ConsoleColor.DarkCyan)
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
                    .AppendLine($"Build-Task '{taskName}' Failed!", ConsoleColor.Red)
                    .AppendLine(result.Message, ConsoleColor.DarkYellow);
            }

            context.Output
                .Append("Build-Task ", ConsoleColor.DarkGreen)
                .Append(taskName, ConsoleColor.Green)
                .Append(" completed successfully.", ConsoleColor.DarkGreen);
        }

        private void MessageClient_OnThreadException(object sender, UnhandledExceptionEventArgs e)
        {
            Log.Error("Unhandled TCP Message Error!", (Exception)e.ExceptionObject);
        }
    }
}
