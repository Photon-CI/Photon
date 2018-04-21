using Photon.Framework.Agent;
using Photon.Framework.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Framework.Tasks
{
    internal class DeployTaskRegistry : TypeRegistry<IDeployTask>
    {
        public async Task ExecuteTask(IAgentDeployContext context, CancellationToken token)
        {
            if (!map.TryGetValue(context.TaskName, out var taskClassType))
                throw new Exception($"Deploy Task '{context.TaskName}' was not found!");

            object classObject = null;
            try {
                classObject = Activator.CreateInstance(taskClassType);

                if (!(classObject is IDeployTask task))
                    throw new Exception($"Unable to create Deploy-Task implementation '{taskClassType}'!");

                task.Context = context;

                await task.RunAsync(token);
            }
            finally {
                (classObject as IDisposable)?.Dispose();
            }
        }
    }
}
