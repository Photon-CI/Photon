using Photon.Framework.Extensions;
using Photon.Framework.Packages;
using Photon.Framework.Projects;
using Photon.Framework.Variables;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Framework.Domain
{
    [Serializable]
    public abstract class DomainContextBase : IDomainContext
    {
        public Project Project {get; set;}
        public string AssemblyFilename {get; set;}
        public string WorkDirectory {get; set;}
        public string BinDirectory {get; set;}
        public string ContentDirectory {get; set;}
        public DomainOutput Output {get; set;}
        public DomainPackageClient Packages {get; set;}
        public VariableSetCollection ServerVariables {get; set;}
        public VariableSetCollection AgentVariables {get; set;}


        public void RunCommandLine(string command)
        {
            Output.WriteBlock()
                .Write("Running Command: ", ConsoleColor.DarkCyan)
                .WriteLine(command, ConsoleColor.Cyan)
                .Post();

            ProcessResult result;
            try {
                result = ProcessRunner.Run(ContentDirectory, command, Output);
            }
            catch (Win32Exception error) when (error.ErrorCode == -2147467259) {
                Output.WriteBlock()
                    .Write("Command Failed!", ConsoleColor.DarkYellow)
                    .WriteLine(" Application not found!", ConsoleColor.Yellow)
                    .Post();

                throw;
            }
            catch (Exception error) {
                Output.WriteBlock()
                    .Write("Command Failed!", ConsoleColor.DarkRed)
                    .WriteLine($" {error.UnfoldMessages()}", ConsoleColor.Red)
                    .Post();

                throw;
            }

            if (result.ExitCode != 0) {
                Output.WriteBlock()
                    .Write("Command Failed! Exit Code ", ConsoleColor.DarkYellow)
                    .WriteLine(result.ExitCode.ToString(), ConsoleColor.Yellow)
                    .Post();

                throw new ApplicationException("Process terminated with a non-zero exit code!");
            }
        }

        public async Task RunCommandLineAsync(string command)
        {
            var taskHandle = new TaskCompletionSource<object>();

            var _ = Task.Run(() => {
                    RunCommandLine(command);
                    return (object) null;
                })
                .ContinueWith(t => taskHandle.FromTask(t));

            await taskHandle.Task;
        }

        public void RunCommandLine(string command, params string[] args)
        {
            var argString = string.Join(" ", args);
            RunCommandLine($"{command} {argString}");
        }

        public async Task RunCommandLineAsync(string command, params string[] args)
        {
            var argString = string.Join(" ", args);
            await RunCommandLineAsync($"{command} {argString}");
        }

        public async Task PushProjectPackageAsync(string filename, CancellationToken token = default(CancellationToken))
        {
            await RemoteTaskCompletionSource.Run(task => {
                Packages.PushProjectPackage(filename, task);
            }, token);
        }

        public async Task PushApplicationPackageAsync(string filename, CancellationToken token = default(CancellationToken))
        {
            await RemoteTaskCompletionSource.Run(task => {
                Packages.PushApplicationPackage(filename, task);
            }, token);
        }

        public async Task<string> PullProjectPackageAsync(string id, string version)
        {
            return await RemoteTaskCompletionSource<string>.Run(task => {
                Packages.PullProjectPackage(id, version, task);
            });
        }

        public async Task<string> PullApplicationPackageAsync(string id, string version)
        {
            return await RemoteTaskCompletionSource<string>.Run(task => {
                Packages.PullApplicationPackage(id, version, task);
            });
        }
    }
}
