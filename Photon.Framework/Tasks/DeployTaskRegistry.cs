using Photon.Framework.Domain;
using System;
using System.Threading.Tasks;

namespace Photon.Framework.Tasks
{
    internal class DeployTaskRegistry : TypeRegistry<IDeployTask>
    {
        public async Task<TaskResult> ExecuteTask(IDeployTaskContext context)
        {
            if (!map.TryGetValue(context.TaskName, out var taskClassType))
                throw new Exception($"Deploy Task '{context.TaskName}' was not found!");

            object classObject = null;
            try {
                classObject = Activator.CreateInstance(taskClassType);
                var task = classObject as IDeployTask;

                return await task.RunAsync(context);
            }
            finally {
                (classObject as IDisposable)?.Dispose();
            }
        }
    }
}
