using System;
using System.Threading.Tasks;
using Photon.Framework.Tasks;

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
    }
}
