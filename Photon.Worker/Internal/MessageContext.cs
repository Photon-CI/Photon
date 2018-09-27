using Photon.Worker.Internal.Session;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Worker.Internal
{
    internal class MessageContext : IDisposable
    {
        private readonly CancellationTokenSource tokenSource;
        private readonly TaskCompletionSource<object> StartTask;
        private readonly TaskCompletionSource<object> CompleteTask;

        public ProcessArguments Arguments {get; set;}
        public WorkerSession Session {get; set;}


        public MessageContext()
        {
            Arguments = new ProcessArguments();
            tokenSource = new CancellationTokenSource();
            StartTask = new TaskCompletionSource<object>();
            CompleteTask = new TaskCompletionSource<object>();
        }

        public void Dispose()
        {
            tokenSource?.Dispose();
        }

        public void Begin()
        {
            StartTask.SetResult(null);
        }

        public void Complete()
        {
            CompleteTask.SetResult(null);
        }

        public void Abort()
        {
            tokenSource.Cancel();
            StartTask.TrySetCanceled();
            CompleteTask.TrySetCanceled();
        }

        public async Task Run()
        {
            var token = tokenSource.Token;
            await Task.Run(async () => await StartTask.Task, token);

            try {
                await Session.Initialize(token);

                await Session.Run(token);
            }
            finally {
                try {
                    await Session.Release(token);
                }
                catch (Exception error) {
                    CompleteTask.SetException(error);
                    throw;
                }
                finally {
                    CompleteTask.SetResult(null);
                }
            }
        }
    }
}
