using Photon.Framework.Domain;
using System;

namespace Photon.Framework.Packages
{
    public delegate void PushPackageFunc(string filename, RemoteTaskCompletionSource<object> taskHandle);

    public delegate void PullPackageFunc(string id, string version, string filename, RemoteTaskCompletionSource<object> taskHandle);

    public class PackageClient : MarshalByRefObject
    {
        public event PushPackageFunc OnPushProjectPackage;
        public event PushPackageFunc OnPushApplicationPackage;
        public event PullPackageFunc OnPullProjectPackage;
        public event PullPackageFunc OnPullApplicationPackage;


        public void PushProjectPackage(string filename, RemoteTaskCompletionSource<object> taskHandle)
        {
            OnPushProjectPackage?.Invoke(filename, taskHandle);
        }

        public void PushApplicationPackage(string filename, RemoteTaskCompletionSource<object> taskHandle)
        {
            OnPushApplicationPackage?.Invoke(filename, taskHandle);
        }

        public void PullProjectPackage(string id, string version, string filename, RemoteTaskCompletionSource<object> taskHandle)
        {
            OnPullProjectPackage?.Invoke(id, version, filename, taskHandle);
        }

        public void PullApplicationPackage(string id, string version, string filename, RemoteTaskCompletionSource<object> taskHandle)
        {
            OnPullApplicationPackage?.Invoke(id, version, filename, taskHandle);
        }
    }
}
