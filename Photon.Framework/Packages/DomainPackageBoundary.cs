﻿using Photon.Framework.Domain;

namespace Photon.Framework.Packages
{
    public delegate void PushPackageFunc(string filename, RemoteTaskCompletionSource taskHandle);

    public delegate void PullPackageFunc(string id, string version, RemoteTaskCompletionSource<string> taskHandle);

    public class DomainPackageBoundary : MarshalByRefInstance
    {
        public event PushPackageFunc OnPushProjectPackage;
        public event PushPackageFunc OnPushApplicationPackage;
        public event PullPackageFunc OnPullProjectPackage;
        public event PullPackageFunc OnPullApplicationPackage;


        public void PushProjectPackage(string filename, RemoteTaskCompletionSource taskHandle)
        {
            OnPushProjectPackage?.Invoke(filename, taskHandle);
        }

        public void PushApplicationPackage(string filename, RemoteTaskCompletionSource taskHandle)
        {
            OnPushApplicationPackage?.Invoke(filename, taskHandle);
        }

        public void PullProjectPackage(string id, string version, RemoteTaskCompletionSource<string> taskHandle)
        {
            OnPullProjectPackage?.Invoke(id, version, taskHandle);
        }

        public void PullApplicationPackage(string id, string version, RemoteTaskCompletionSource<string> taskHandle)
        {
            OnPullApplicationPackage?.Invoke(id, version, taskHandle);
        }
    }
}
