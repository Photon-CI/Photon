using Photon.Framework.Domain;
using System.Threading;
using System.Threading.Tasks;

namespace Photon.Framework.Applications
{
    public delegate void GetApplicationRevisionFunc(string projectId, string appName, uint deploymentNumber, RemoteTaskCompletionSource<DomainApplicationRevision> taskHandle);
    public delegate void RegisterApplicationRevisionFunc(DomainApplicationRevisionRequest appRevisionRequest, RemoteTaskCompletionSource<DomainApplicationRevision> taskHandle);

    public interface IDomainApplicationClient
    {
        Task<DomainApplicationRevision> GetApplicationRevision(string projectId, string appName, uint deploymentNumber, CancellationToken token = default(CancellationToken));
        Task<DomainApplicationRevision> RegisterApplicationRevision(DomainApplicationRevisionRequest appRevisionRequest, CancellationToken token = default(CancellationToken));
    }

    public class DomainApplicationClient : IDomainApplicationClient
    {
        public event GetApplicationRevisionFunc OnGetApplicationRevision;
        public event RegisterApplicationRevisionFunc OnRegisterApplicationRevision;


        public async Task<DomainApplicationRevision> GetApplicationRevision(string projectId, string appName, uint deploymentNumber, CancellationToken token = default(CancellationToken))
        {
            return await RemoteTaskCompletionSource<DomainApplicationRevision>.Run(task => {
                OnGetApplicationRevision?.Invoke(projectId, appName, deploymentNumber, task);
            }, token);
        }

        public async Task<DomainApplicationRevision> RegisterApplicationRevision(DomainApplicationRevisionRequest appRevisionRequest, CancellationToken token = default(CancellationToken))
        {
            return await RemoteTaskCompletionSource<DomainApplicationRevision>.Run(task => {
                OnRegisterApplicationRevision?.Invoke(appRevisionRequest, task);
            }, token);
        }
    }
}
