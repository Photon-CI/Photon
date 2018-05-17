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

        public void RunBuildTask(IAgentBuildContext context, RemoteTaskCompletionSource completeEvent)
        {
            buildTaskRegistry.ExecuteTask(context, CancellationToken.None)
                .ContinueWith(completeEvent.FromTask);
        }

        //class TaskRoleException : ApplicationException
        //{
        //    //
        //}

        public void RunDeployTask(IAgentDeployContext context, RemoteTaskCompletionSource completeEvent)
        {
            if (deployTaskRegistry.TryGetDescription(context.TaskName, out var taskDesc)) {
                if (taskDesc.Roles?.Any() ?? false) {
                    var isInRole = context.Agent?.Roles?.ContainsAny(taskDesc.Roles, StringComparer.OrdinalIgnoreCase) ?? false;

                    if (!isInRole) {
                        // Task is not in agent roles
                        completeEvent.SetResult();
                        return;
                    }
                }
            }

            context.Output.Append("Running deployment task ", ConsoleColor.DarkCyan)
                .Append(context.TaskName, ConsoleColor.Cyan)
                .Append(" on agent ", ConsoleColor.DarkCyan)
                .Append(context.Agent?.Name, ConsoleColor.Cyan)
                .AppendLine("...", ConsoleColor.DarkCyan);

            deployTaskRegistry.ExecuteTask(context, CancellationToken.None)
                .ContinueWith(completeEvent.FromTask);
        }
    }
}
