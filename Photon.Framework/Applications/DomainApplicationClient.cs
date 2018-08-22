using Photon.Framework.Domain;

namespace Photon.Framework.Applications
{
    public delegate void GetApplicationRevisionFunc(string projectId, string appName, uint deploymentNumber, RemoteTaskCompletionSource<DomainApplicationRevision> taskHandle);
    public delegate void RegisterApplicationRevisionFunc(DomainApplicationRevisionRequest appRevisionRequest, RemoteTaskCompletionSource<DomainApplicationRevision> taskHandle);

    public class DomainApplicationClient : MarshalByRefInstance
    {
        public event GetApplicationRevisionFunc OnGetApplicationRevision;
        public event RegisterApplicationRevisionFunc OnRegisterApplicationRevision;


        public void GetApplicationRevision(string projectId, string appName, uint deploymentNumber, RemoteTaskCompletionSource<DomainApplicationRevision> taskHandle)
        {
            OnGetApplicationRevision?.Invoke(projectId, appName, deploymentNumber, taskHandle);
        }

        public void RegisterApplicationRevision(DomainApplicationRevisionRequest appRevisionRequest, RemoteTaskCompletionSource<DomainApplicationRevision> taskHandle)
        {
            OnRegisterApplicationRevision?.Invoke(appRevisionRequest, taskHandle);
        }
    }
}
