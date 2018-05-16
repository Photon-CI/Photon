using Photon.Framework.Agent;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Framework.Tasks
{
    internal class BuildTaskRegistry : TaskRegistryBase<IBuildTask>
    {
        public async Task ExecuteTask(IAgentBuildContext context, CancellationToken token)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (string.IsNullOrEmpty(context.TaskName)) throw new ArgumentException("TaskName is undefined!");

            if (!mapType.TryGetValue(context.TaskName, out var taskType))
                throw new Exception($"Build Task '{context.TaskName}' was not found!");

            object classObject = null;
            try {
                classObject = Activator.CreateInstance(taskType);

                if (!(classObject is IBuildTask task))
                    throw new Exception($"Invalid BuildTask implementation '{taskType.Name}'!");

                task.Context = context;

                await task.RunAsync(token);
            }
            finally {
                (classObject as IDisposable)?.Dispose();
            }
        }
    }
}
