using Photon.Framework.Agent;
using Photon.Framework.Domain;
using System;
using System.Threading.Tasks;

namespace Photon.Framework.Tasks
{
    internal class DeployTaskRegistry : TypeRegistry<IDeployTask>
    {
        public async Task<TaskResult> ExecuteTask(IAgentDeployContext context)
        {
            if (!map.TryGetValue(context.TaskName, out var taskClassType))
                throw new Exception($"Deploy Task '{context.TaskName}' was not found!");

            object classObject = null;
            try {
                classObject = Activator.CreateInstance(taskClassType);

                if (!(classObject is IDeployTask task))
                    throw new Exception($"Unable to create Deploy-Task implementation '{taskClassType}'!");

                task.Context = context;

                return await task.RunAsync();
            }
            finally {
                (classObject as IDisposable)?.Dispose();
            }
        }
    }
}
