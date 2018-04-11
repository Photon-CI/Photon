using Photon.Framework.Domain;
using System.Threading.Tasks;

namespace Photon.Framework.Extensions
{
    public static class RemoteTaskExtensions
    {
        public static void FromTask<T>(this RemoteTaskCompletionSource<T> completionSource, Task<T> task)
        {
            if (task.IsCanceled) completionSource.SetCancelled();
            else if (task.IsFaulted) completionSource.SetException(task.Exception);
            else completionSource.SetResult(task.Result);
        }
    }
}
