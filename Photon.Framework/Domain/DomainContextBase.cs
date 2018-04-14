using Photon.Framework.Packages;
using Photon.Framework.Projects;
using Photon.Framework.Server;
using System;
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
        public ScriptOutput Output {get; set;}
        public DomainPackageClient Packages {get; set;}


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

        public async Task PushProjectPackageAsync(string filename)
        {
            await RemoteTaskCompletionSource<object>.Run((task, sponsor) => {
                Packages.PushProjectPackage(filename, task);
            });
        }

        public async Task PushApplicationPackageAsync(string filename)
        {
            await RemoteTaskCompletionSource<object>.Run((task, sponsor) => {
                Packages.PushApplicationPackage(filename, task);
            });
        }

        public async Task<string> PullProjectPackageAsync(string id, string version)
        {
            return await RemoteTaskCompletionSource<string>.Run((task, sponsor) => {
                Packages.PullProjectPackage(id, version, task);
            });
        }

        public async Task<string> PullApplicationPackageAsync(string id, string version)
        {
            return await RemoteTaskCompletionSource<string>.Run((task, sponsor) => {
                Packages.PullApplicationPackage(id, version, task);
            });
        }
    }
}
