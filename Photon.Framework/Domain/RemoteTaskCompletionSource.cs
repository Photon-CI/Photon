using System;
using System.Runtime.Remoting.Lifetime;
using System.Threading.Tasks;

namespace Photon.Framework.Domain
{
    public class RemoteTaskCompletionSource<T> : MarshalByRefObject
    {
        private readonly TaskCompletionSource<T> taskEvent;

        public Task<T> Task => taskEvent.Task;


        public RemoteTaskCompletionSource()
        {
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

        public static async Task<T> Run(Action<RemoteTaskCompletionSource<T>, ISponsor> action)
        {
            var sponsor = new ClientSponsor();

            try {
                var taskHandle = new RemoteTaskCompletionSource<T>();
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
