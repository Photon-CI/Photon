using System;
using System.Runtime.Remoting.Lifetime;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Framework.Domain
{
    public class RemoteTaskCompletionSource : MarshalByRefObject
    {
        private readonly TaskCompletionSource<object> taskEvent;

        public CancellationToken Token {get;}
        public Task Task => taskEvent.Task;


        public RemoteTaskCompletionSource(CancellationToken token = default(CancellationToken))
        {
            this.Token = token;

            taskEvent = new TaskCompletionSource<object>();
        }

        public void SetResult()
        {
            taskEvent.SetResult(null);
        }

        public void SetCancelled()
        {
            taskEvent.SetCanceled();
        }

        public void SetException(Exception error)
        {
            taskEvent.SetException(error);
        }

        public static async Task Run(Action<RemoteTaskCompletionSource, ISponsor> action, CancellationToken token = default(CancellationToken))
        {
            var sponsor = new ClientSponsor();

            try {
                var taskHandle = new RemoteTaskCompletionSource(token);
                sponsor.Register(taskHandle);

                action(taskHandle, sponsor);
                await taskHandle.Task;
            }
            finally {
                sponsor.Close();
            }
        }
    }

    public class RemoteTaskCompletionSource<T> : MarshalByRefObject
    {
        private readonly TaskCompletionSource<T> taskEvent;

        public CancellationToken Token {get;}
        public Task<T> Task => taskEvent.Task;


        public RemoteTaskCompletionSource(CancellationToken token = default(CancellationToken))
        {
            this.Token = token;

            taskEvent = new TaskCompletionSource<T>();
        }

        public void SetResult(T result)
        {
            taskEvent.SetResult(result);
        }

        public void SetCancelled()
        {
            taskEvent.SetCanceled();
        }

        public void SetException(Exception error)
        {
            taskEvent.SetException(error);
        }

        public static async Task<T> Run(Action<RemoteTaskCompletionSource<T>, ISponsor> action, CancellationToken token = default(CancellationToken))
        {
            var sponsor = new ClientSponsor();

            try {
                var taskHandle = new RemoteTaskCompletionSource<T>(token);
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
