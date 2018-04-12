using Photon.Framework.Domain;
using Photon.Framework.Packages;
using Photon.Framework.Projects;
using Photon.Framework.Server;
using System;
using System.Runtime.Remoting.Lifetime;
using System.Threading.Tasks;

namespace Photon.Framework.Agent
{
    [Serializable]
    public abstract class AgentContextBase : IAgentContext
    {
        public Project Project {get; set;}
        public string AssemblyFilename {get; set;}
        public string WorkDirectory {get; set;}
        public string ContentDirectory {get; set;}
        public string BinDirectory {get; set;}
        public ScriptOutput Output {get; set;}
        public PackageClient Packages {get; set;}


        public async Task PushProjectPackageAsync(string filename)
        {
            await RunRemoteTask(task => {
                Packages.PushProjectPackage(filename, task);
            });
        }

        public async Task PushApplicationPackageAsync(string filename)
        {
            await RunRemoteTask(task => {
                Packages.PushApplicationPackage(filename, task);
            });
        }

        public async Task PullProjectPackageAsync(string id, string version, string filename)
        {
            await RunRemoteTask(task => {
                Packages.PullProjectPackage(id, version, filename, task);
            });
        }

        public async Task PullApplicationPackageAsync(string id, string version, string filename)
        {
            await RunRemoteTask(task => {
                Packages.PullApplicationPackage(id, version, filename, task);
            });
        }

        public void RunCommandLine(string command)
        {
            Output.Append("Running Command: ", ConsoleColor.DarkCyan)
                .AppendLine(command, ConsoleColor.Cyan);

            var result = ProcessRunner.Run(ContentDirectory, command, Output);

            if (result.ExitCode != 0) {
                Output.Append("Command Failed! Exit Code ", ConsoleColor.DarkYellow)
                    .AppendLine(result.ExitCode.ToString(), ConsoleColor.Yellow);

                throw new ApplicationException("Process terminated with a non-zero exit code!");
            }
        }

        public void RunCommandLine(string command, params string[] args)
        {
            var argString = string.Join(" ", args);
            RunCommandLine($"{command} {argString}");
        }

        private static async Task RunRemoteTask(Action<RemoteTaskCompletionSource<object>> action)
        {
            var sponsor = new ClientSponsor();

            try {
                var taskHandle = new RemoteTaskCompletionSource<object>();
                sponsor.Register(taskHandle);

                action(taskHandle);
                await taskHandle.Task;
            }
            finally {
                sponsor.Close();
            }
        }
    }
}
