using Photon.Framework.Domain;
using System;
using System.Threading.Tasks;

namespace Photon.Framework.Tasks
{
    internal class BuildTaskRegistry : TypeRegistry<IBuildTask>
    {
        public async Task<TaskResult> ExecuteTask(IAgentBuildContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (string.IsNullOrEmpty(context.TaskName)) throw new ArgumentException("TaskName is undefined!");

            if (!map.TryGetValue(context.TaskName, out var taskClassType))
                throw new Exception($"Build Task '{context.TaskName}' was not found!");

            object classObject = null;
            try {
                classObject = Activator.CreateInstance(taskClassType);
                var task = classObject as IBuildTask;

                if (task == null) throw new Exception($"Invalid BuildTask implementation '{taskClassType.Name}'!");

                return await task.RunAsync(context);
            }
            finally {
                (classObject as IDisposable)?.Dispose();
            }
        }
    }
}
