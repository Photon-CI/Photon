using Photon.Framework.Agent;
using Photon.Framework.Process;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Worker.Internal.Session
{
    internal class WorkerBuildSession : WorkerSession
    {
        public IAgentBuildContext BuildContext => (IAgentBuildContext)Context;


        public override async Task Initialize(CancellationToken token = default)
        {
            await base.Initialize(token);

            LoadProjectAssembly();
        }

        public override Task Run(CancellationToken token = default)
        {
            //...

            return base.Run(token);
        }

        private void LoadProjectAssembly()
        {
            if (string.IsNullOrEmpty(BuildContext.AssemblyFilename)) {
                Output.WriteLine("No assembly filename defined!", ConsoleColor.DarkRed);
                throw new ApplicationException("Assembly filename is undefined!");
            }

            if (!string.IsNullOrWhiteSpace(BuildContext.PreBuildCommand)) {
                Output.WriteLine("Running Pre-Build Script...", ConsoleColor.DarkCyan);

                try {
                    var runInfo = ProcessRunInfo.FromCommand(BuildContext.PreBuildCommand);
                    runInfo.WorkingDirectory = Context.ContentDirectory;

                    var result = new ProcessRunner {
                        Output = Output,
                    }.Run(runInfo);

                    if (result.ExitCode != 0)
                        throw new ApplicationException("Process terminated with a non-zero exit code!");
                }
                catch (Exception error) {
                    Output.WriteLine($"An error occurred while executing the Pre-Build script! {error.Message} [{Context.AgentSessionId}]", ConsoleColor.DarkYellow);
                    throw new ApplicationException($"Script Pre-Build failed! [{Context.AgentSessionId}]", error);
                }
            }

            var assemblyFilename = Path.Combine(Context.ContentDirectory, Context.AssemblyFilename);

            if (!File.Exists(assemblyFilename)) {
                Output.WriteLine($"The assembly file '{assemblyFilename}' could not be found!", ConsoleColor.DarkYellow);
                throw new ApplicationException($"The assembly file '{assemblyFilename}' could not be found!");
            }

            var assemblyCopyFilename = ShadowCopyAssembly();

            try {
                Assembly.LoadFile(assemblyCopyFilename);
            }
            catch (Exception error) {
                Output.WriteLine($"An error occurred while loading the assembly! {error.Message} [{Context.AgentSessionId}]", ConsoleColor.DarkRed);
                throw new ApplicationException($"Failed to load Assembly! [{Context.AgentSessionId}]", error);
            }
        }
    }
}
