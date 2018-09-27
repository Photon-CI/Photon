using Photon.Framework.Agent;
using Photon.Framework.Tasks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Worker.Internal.Tasks
{
    internal class DeployTaskRegistry : TaskRegistryBase<IDeployTask>
    {
        public async Task ExecuteTask(IAgentDeployContext context, CancellationToken token)
        {
            if (!mapType.TryGetValue(context.TaskName, out var taskType))
                throw new Exception($"Deploy Task '{context.TaskName}' was not found!");

            object classObject = null;
            try {
                classObject = Activator.CreateInstance(taskType);

                if (!(classObject is IDeployTask task))
                    throw new Exception($"Unable to create Deploy-Task implementation '{taskType.Name}'!");

                task.Context = context;

                await task.RunAsync(token);
            }
            finally {
                (classObject as IDisposable)?.Dispose();
            }
        }
    }
}
