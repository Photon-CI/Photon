using Photon.Framework.Domain;
using System;
using System.Threading.Tasks;

namespace Photon.Framework.Tasks
{
    internal class TaskRegistry : TypeRegistry<ITask>
    {
        public async Task<TaskResult> ExecuteTask(BuildTaskContext context)
        {
            if (!map.TryGetValue(context.TaskName, out var taskClassType))
                throw new Exception($"Task '{context.TaskName}' was not found!");

            object classObject = null;
            try {
                classObject = Activator.CreateInstance(taskClassType);
                var task = classObject as ITask;

                return await task.RunAsync(context);
            }
            finally {
                (classObject as IDisposable)?.Dispose();
            }
        }
    }
}
