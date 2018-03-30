using Photon.Framework.Domain;
using System;
using System.Threading.Tasks;

namespace Photon.Framework.Tasks
{
    internal class TaskRegistry : TypeRegistry<ITask>
    {
        public async Task<TaskResult> ExecuteTask(TaskContext context)
        {
            if (!map.TryGetValue(context.Task.Name, out var taskClassType))
                throw new Exception($"Task '{context.Task.Name}' was not found!");

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
