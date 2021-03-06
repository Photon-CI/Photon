﻿using Photon.Framework.Domain;
using System.Threading.Tasks;

namespace Photon.Framework.Extensions
{
    public static class TaskExtensions
    {
        public static void FromTask<T>(this TaskCompletionSource<T> completionSource, Task<T> task)
        {
            if (task.IsCanceled) completionSource.SetCanceled();
            else if (task.IsFaulted) completionSource.SetException(task.Exception);
            else completionSource.SetResult(task.Result);
        }

        public static void FromTask(this RemoteTaskCompletionSource completionSource, Task task)
        {
            if (task.IsCanceled) completionSource.SetCancelled();
            else if (task.IsFaulted) completionSource.SetException(task.Exception);
            else completionSource.SetResult();
        }

        public static void FromTask<T>(this RemoteTaskCompletionSource<T> completionSource, Task<T> task)
        {
            if (task.IsCanceled) completionSource.SetCancelled();
            else if (task.IsFaulted) completionSource.SetException(task.Exception);
            else completionSource.SetResult(task.Result);
        }
    }
}
