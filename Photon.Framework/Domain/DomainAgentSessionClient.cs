using System;

namespace Photon.Framework.Domain
{
    public delegate void SessionBeginFunc(RemoteTaskCompletionSource taskHandle);
    public delegate void SessionReleaseFunc(RemoteTaskCompletionSource taskHandle);
    public delegate void SessionRunTaskFunc(string taskName, RemoteTaskCompletionSource taskHandle);
    public delegate void SessionGetTaskRolesFunc(string taskName, RemoteTaskCompletionSource<string[]> taskHandle);
    public delegate void SessionDisposedFunc();

    public class DomainAgentSessionClient : MarshalByRefObject
    {
        public string Id {get;}
        public event SessionBeginFunc OnSessionBegin;
        public event SessionReleaseFunc OnSessionRelease;
        public event SessionRunTaskFunc OnSessionRunTask;
        public event SessionGetTaskRolesFunc OnSessionGetTaskRoles;
        public event SessionDisposedFunc OnSessionDisposed;


        public DomainAgentSessionClient()
        {
            Id = Guid.NewGuid().ToString("N");
        }

        public void Begin(RemoteTaskCompletionSource taskHandle)
        {
            OnSessionBegin?.Invoke(taskHandle);
        }

        public void ReleaseAsync(RemoteTaskCompletionSource taskHandle)
        {
            OnSessionRelease?.Invoke(taskHandle);
        }

        public void RunTaskAsync(string taskName, RemoteTaskCompletionSource taskHandle)
        {
            OnSessionRunTask?.Invoke(taskName, taskHandle);
        }

        public void GetTaskRolesAsync(string taskName, RemoteTaskCompletionSource<string[]> taskHandle)
        {
            OnSessionGetTaskRoles?.Invoke(taskName, taskHandle);
        }

        public void Dispose()
        {
            OnSessionDisposed?.Invoke();
        }
    }
}
