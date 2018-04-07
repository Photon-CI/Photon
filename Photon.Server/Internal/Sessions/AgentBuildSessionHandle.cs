using Photon.Communication;
using Photon.Framework;
using Photon.Framework.Messages;
using Photon.Framework.Projects;
using Photon.Framework.Scripts;
using Photon.Server.Internal.Tasks;
using System;
using System.Reflection;
using System.Threading.Tasks;
using log4net;

namespace Photon.Server.Internal.Sessions
{
    public class AgentBuildSessionHandle : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(AgentBuildSessionHandle));

        private readonly ServerAgentDefinition definition;
        private readonly MessageClient messageClient;

        private string agentSessionId;

        public string ServerSessionId {get; set;}
        public Project Project {get; set;}
        public string TaskName {get; set;}
        public string AssemblyFile {get; set;}
        public string GitRefspec {get; set;}
        public int BuildNumber {get; set;}
        public ScriptOutput Output {get; set;}


        public AgentBuildSessionHandle(ServerAgentDefinition agentDefinition)
        {
            this.definition = agentDefinition;

            // TODO: This can be moved to single instance in PhotonServer
            var registry = new MessageRegistry();
            registry.Scan(Assembly.GetExecutingAssembly());

            messageClient = new MessageClient(registry);
        }

        public void Dispose()
        {
            messageClient?.Dispose();
        }

        public async Task BeginSessionAsync()
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
                ServerSessionId = ServerSessionId,
                Project = Project,
                AssemblyFile = AssemblyFile,
                GitRefspec = GitRefspec,
                BuildNumber = BuildNumber,
                TaskName = TaskName,
            };

            var handle = messageClient.Send(message);
            var response = await handle.GetResponseAsync<BuildSessionBeginResponse>();

            if (!response.Successful)
                throw new ApplicationException($"Failed to start Agent Session! {response.Exception}");

            agentSessionId = response.SessionId;
            if (string.IsNullOrEmpty(agentSessionId))
                throw new ApplicationException("Failed to begin agent session!");
        }

        public async Task ReleaseSessionAsync()
        {
            // TODO: Locking on sessionId and isActive

            if (messageClient.IsConnected) {
                var message = new BuildSessionReleaseRequest {
                    SessionId = agentSessionId,
                };

                var handle = messageClient.Send(message);
                var response = await handle.GetResponseAsync<BuildSessionReleaseResponse>();

                if (!(response?.Successful ?? false))
                    throw new ApplicationException("Failed to release agent session!");

                await messageClient.DisconnectAsync();
            }
        }

        public async Task RunTaskAsync()
        {
            Output
                .Append("Running Task ", ConsoleColor.DarkCyan)
                .Append(TaskName, ConsoleColor.Cyan)
                .Append(" on Agent ", ConsoleColor.DarkCyan)
                .Append(definition.Name, ConsoleColor.Cyan)
                .AppendLine("...", ConsoleColor.DarkCyan);

            var runner = new TaskRunner(messageClient, agentSessionId);
            runner.OutputEvent += (o, e) => {
                Output.AppendRaw(e.Text);
            };

            PhotonServer.Instance.TaskRunners.Add(runner);

            var result = await runner.Run(TaskName);

            if (!result.Successful) {
                Output
                    .AppendLine($"Task '{TaskName}' Failed!", ConsoleColor.Red)
                    .AppendLine(result.Message, ConsoleColor.DarkYellow);
            }

            Output
                .Append("Task ", ConsoleColor.DarkGreen)
                .Append(TaskName, ConsoleColor.Green)
                .Append(" completed successfully.", ConsoleColor.DarkGreen);
        }
    }
}
