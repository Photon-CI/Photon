using Photon.Framework;
using Photon.Library;
using System;
using System.Threading.Tasks;

namespace Photon.Agent.Internal
{
    internal static class TaskRunner
    {
        public static async Task<TaskResult> StartTask(ContextDefinition taskContext)
        {
            var response = new TaskStartResponseDefinition {
                Agent = {
                    Name = Program.Agent?.Definition?.Name,
                },
            };

            try {
                await RunTask(taskContext);

                response.Successful = true;
                response.Message = "Ok";
            }
            catch (Exception error) {
                response.Successful = false;
                response.Message = error.ToString();
            }

            return response;
        }

        private static async Task<TaskResult> RunTask(ContextDefinition taskContext)
        {
            var context = new TaskContext(taskContext);

            context.Agent.Name = Program.Agent?.Definition?.Name;
            //...

            // TODO: RUN

            return new TaskResult {
                Successful = false,
                Cancelled = true,
                Message = "Not Yet Implemented!",
            };
        }
    }
}
