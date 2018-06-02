using System;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Framework.Domain
{
    public class RemoteTaskCompletionSource : MarshalByRefInstance
    {
        private readonly TaskCompletionSource<object> taskEvent;
        private volatile bool isComplete;

        public Task Task => taskEvent.Task;


        public RemoteTaskCompletionSource()
        {
            isComplete = false;
            taskEvent = new TaskCompletionSource<object>();
        }

        public void SetResult()
        {
            if (isComplete) return;
            isComplete = true;

            taskEvent.SetResult(null);
        }

        public void SetCancelled()
        {
            if (isComplete) return;
            isComplete = true;

            taskEvent.SetCanceled();
        }

        public void SetException(Exception error)
        {
            if (isComplete) return;
            isComplete = true;

            taskEvent.SetException(error);
        }

        public static async Task Run(Action<RemoteTaskCompletionSource> action, CancellationToken token = default(CancellationToken))
        {
            using (var taskHandle = new RemoteTaskCompletionSource()) {
                var _task = new WeakReference(taskHandle);
                token.Register(() => {
                    if (!_task.IsAlive) return;
                    ((RemoteTaskCompletionSource) _task.Target).SetCancelled();
                });

                action(taskHandle);
                await taskHandle.Task;
            }
        }
    }

    public class RemoteTaskCompletionSource<T> : MarshalByRefInstance
    {
        private readonly TaskCompletionSource<T> taskEvent;
        private volatile bool isComplete;

        public Task<T> Task => taskEvent.Task;


        public RemoteTaskCompletionSource()
        {
            taskEvent = new TaskCompletionSource<T>();
        }

        public void SetResult(T result)
        {
            if (isComplete) return;
            isComplete = true;

            taskEvent.SetResult(result);
        }

        public void SetCancelled()
        {
            if (isComplete) return;
            isComplete = true;

            taskEvent.SetCanceled();
        }

        public void SetException(Exception error)
        {
            if (isComplete) return;
            isComplete = true;

            taskEvent.SetException(error);
        }

        public static async Task<T> Run(Action<RemoteTaskCompletionSource<T>> action, CancellationToken token = default(CancellationToken))
        {
            using (var taskHandle = new RemoteTaskCompletionSource<T>()) {
                var _task = new WeakReference(taskHandle);
                token.Register(() => {
                    if (!_task.IsAlive) return;
                    ((RemoteTaskCompletionSource) _task.Target).SetCancelled();
                });

                action(taskHandle);
                return await taskHandle.Task;
            }
        }
    }
}
