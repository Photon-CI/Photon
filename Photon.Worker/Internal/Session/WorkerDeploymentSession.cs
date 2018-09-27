using Photon.Framework.Agent;
using Photon.Worker.Internal.Tasks;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Worker.Internal.Session
{
    internal class WorkerDeploymentSession : WorkerSession
    {
        private readonly DeployTaskRegistry deployTaskRegistry;

        public AgentDeployContext DeployContext {get; set;}
        public override IAgentContext Context => DeployContext;


        public WorkerDeploymentSession()
        {
            deployTaskRegistry = new DeployTaskRegistry();
        }

        public override async Task Initialize(CancellationToken token = default)
        {
            await base.Initialize(token);

            LoadProjectAssembly();
        }

        public override Task Run(CancellationToken token = default)
        {


            return base.Run(token);
        }

        public override Task Release(CancellationToken token = default)
        {
            return base.Release(token);
        }

        private void LoadProjectAssembly()
        {
            var _assemblyFile = Path.Combine(Context.BinDirectory, Context.AssemblyFilename);

            try {
                var assembly = Assembly.LoadFile(_assemblyFile);

                deployTaskRegistry.ScanAssembly(assembly);
            }
            catch (Exception error) {
                throw;
            }
        }

        public async Task RunDeployTask(string taskName, CancellationToken token = default)
        {
            using (var block = Output.WriteBlock()) {
                block.Write("Running deployment task ", ConsoleColor.DarkCyan);
                block.Write(taskName, ConsoleColor.Cyan);
                block.Write(" on agent ", ConsoleColor.DarkCyan);
                block.Write(Context.Agent?.Name, ConsoleColor.Cyan);
                block.WriteLine("...", ConsoleColor.DarkCyan);
            }

            try {
                await deployTaskRegistry.ExecuteTask(DeployContext, token);
                completeTask.SetResult(null);
            }
            catch (Exception error) {
                completeTask.SetException(error);
                throw;
            }
        }
    }
}
