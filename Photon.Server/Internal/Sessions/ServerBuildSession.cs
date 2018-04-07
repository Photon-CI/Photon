using Photon.Framework;
using Photon.Framework.Projects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Photon.Server.Internal.Sessions
{
    internal class ServerBuildSession : ServerSessionBase
    {
        private AgentBuildSessionHandle sessionHandle;

        //private readonly ReferencePool<TaskRunner> runningTasks;

        public Project Project {get; set;}
        public string AssemblyFile {get; set;}
        public string TaskName {get; set;}
        public string GitRefspec {get; set;}
        public string[] Roles {get; set;}
        public int BuildNumber {get; set;}


        public override void Dispose()
        {
            //runningTasks?.Dispose();
            sessionHandle?.Dispose();

            base.Dispose();
        }

        public override async Task RunAsync()
        {
            using (sessionHandle = RegisterAgent(Roles)) {
                try {
                    await sessionHandle.BeginSessionAsync();

                    await sessionHandle.RunTaskAsync();
                }
                catch (Exception error) {
                    Exception = error;
                    throw;
                }
                finally {
                    await sessionHandle.ReleaseSessionAsync();
                }
            }
        }

        private AgentBuildSessionHandle RegisterAgent(params string[] roles)
        {
            if (roles == null) throw new ArgumentNullException(nameof(roles));

            var roleAgents = PhotonServer.Instance.Definition
                .Agents.Where(a => a.MatchesRoles(roles)).ToArray();

            if (!roleAgents.Any())
                throw new ApplicationException($"No Agents found in roles '{string.Join("; ", roles)}'!");

            PrintFoundAgents(roleAgents);

            ServerAgentDefinition agent;
            if (roleAgents.Length == 1) {
                agent = roleAgents[0];
            }
            else {
                var random = new Random();
                agent = roleAgents[random.Next(roleAgents.Length)];
            }

            return new AgentBuildSessionHandle(agent) {
                ServerSessionId = SessionId,
                Project = Project,
                AssemblyFile = AssemblyFile,
                TaskName = TaskName,
                GitRefspec = GitRefspec,
                BuildNumber = BuildNumber,
                Output = Output,
            };
        }

        private void PrintFoundAgents(IEnumerable<ServerAgentDefinition> agents)
        {
            var agentNames = agents.Select(x => x.Name);
            Output.Append("Found Agents: ", ConsoleColor.DarkCyan);

            var i = 0;
            foreach (var name in agentNames) {
                if (i > 0) Output.Append("; ", ConsoleColor.DarkCyan);
                i++;

                Output.Append(name, ConsoleColor.Cyan);
            }

            Output.AppendLine(".", ConsoleColor.DarkCyan);
        }
    }
}
