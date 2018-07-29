using Photon.Framework.Agent;
using Photon.Framework.Extensions;
using Photon.Framework.Tasks;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Photon.Framework.Domain
{
    public class AgentDomainAgent : DomainAgentBase
    {
        private readonly BuildTaskRegistry buildTaskRegistry;
        private readonly DeployTaskRegistry deployTaskRegistry;


        public AgentDomainAgent()
        {
            buildTaskRegistry = new BuildTaskRegistry();
            deployTaskRegistry = new DeployTaskRegistry();
        }

        protected override void OnAssemblyLoaded(Assembly assembly)
        {
            base.OnAssemblyLoaded(assembly);

            buildTaskRegistry.ScanAssembly(assembly);
            deployTaskRegistry.ScanAssembly(assembly);
        }

        public TaskDescription[] GetBuildTasks()
        {
            return buildTaskRegistry.AllDescriptions.ToArray();
        }

        public TaskDescription[] GetDeployTasks()
        {
            return deployTaskRegistry.AllDescriptions.ToArray();
        }

        public bool IsBuildTaskInRole(IAgentBuildContext context)
        {
            if (!buildTaskRegistry.TryGetDescription(context.TaskName, out var taskDesc))
                return true;

            if (!(taskDesc.Roles?.Any() ?? false)) return true;

            return context.Agent?.Roles?
                .ContainsAny(taskDesc.Roles, StringComparer.OrdinalIgnoreCase) ?? false;
        }

        public void RunBuildTask(IAgentBuildContext context, RemoteTaskCompletionSource completeEvent)
        {
            buildTaskRegistry.ExecuteTask(context, CancellationToken.None)
                .ContinueWith(completeEvent.FromTask);
        }

        public bool IsDeployTaskInRole(IAgentDeployContext context)
        {
            if (!deployTaskRegistry.TryGetDescription(context.TaskName, out var taskDesc))
                return true;

            if (!(taskDesc.Roles?.Any() ?? false)) return true;

            return context.Agent?.Roles?
                .ContainsAny(taskDesc.Roles, StringComparer.OrdinalIgnoreCase) ?? false;
        }

        public void RunDeployTask(IAgentDeployContext context, RemoteTaskCompletionSource completeEvent)
        {
            using (var block = context.Output.WriteBlock()) {
                block.Write("Running deployment task ", ConsoleColor.DarkCyan);
                block.Write(context.TaskName, ConsoleColor.Cyan);
                block.Write(" on agent ", ConsoleColor.DarkCyan);
                block.Write(context.Agent?.Name, ConsoleColor.Cyan);
                block.WriteLine("...", ConsoleColor.DarkCyan);
            }

            deployTaskRegistry.ExecuteTask(context, CancellationToken.None)
                .ContinueWith(completeEvent.FromTask);
        }
    }
}
