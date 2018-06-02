using System;
using System.Runtime.Remoting.Lifetime;
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

        public static async Task Run(Action<RemoteTaskCompletionSource, ISponsor> action, CancellationToken token = default(CancellationToken))
        {
            var sponsor = new ClientSponsor();
            var taskHandle = new RemoteTaskCompletionSource();

            token.Register(() => taskHandle.SetCancelled());

            try {
                sponsor.Register(taskHandle);

                action(taskHandle, sponsor);
                await taskHandle.Task;
            }
            finally {
                sponsor.Close();
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

        public static async Task<T> Run(Action<RemoteTaskCompletionSource<T>, ISponsor> action, CancellationToken token = default(CancellationToken))
        {
            var sponsor = new ClientSponsor();
            var taskHandle = new RemoteTaskCompletionSource<T>();

            token.Register(() => taskHandle.SetCancelled());

            try {
                sponsor.Register(taskHandle);

                action(taskHandle, sponsor);
                return await taskHandle.Task;
            }
            finally {
                sponsor.Close();
            }
        }
    }
}
